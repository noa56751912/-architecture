using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Services
{
    /// <summary>
    /// Wraps a Confluent <see cref="IProducer{TKey,TValue}"/> built with
    /// <see cref="ProducerBuilder{TKey,TValue}"/> from settings bound via
    /// <see cref="KafkaSettings"/>.
    /// Registered as a singleton so the underlying producer is reused.
    /// </summary>
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topicName;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IOptions<KafkaSettings> options, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            var settings = options.Value;
            _topicName = settings.TopicName;

            // Build the producer from a ProducerConfig that contains all connection details from appsettings.
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                Acks = Acks.All,                   // wait for full ISR acknowledgement
                EnableIdempotence = true,          // exactly-once delivery semantics
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 500
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, error) =>
                    _logger.LogError("Kafka producer error: {Reason} (fatal={IsFatal})", error.Reason, error.IsFatal))
                .Build();
        }

        public async Task PublishOrderEventAsync(OrderEventMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = message.OrderId.ToString(),
                Value = json
            };

            var result = await _producer.ProduceAsync(_topicName, kafkaMessage);
            _logger.LogInformation(
                "Order event published → topic={Topic} partition={Partition} offset={Offset} OrderId={OrderId}",
                result.Topic, result.Partition.Value, result.Offset.Value, message.OrderId);
        }

        public void Dispose()
        {
            // Flush ensures any buffered messages are sent before disposal.
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}
