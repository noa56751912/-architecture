namespace Services
{
    /// <summary>
    /// Strongly-typed binding for the "Kafka" section in appsettings.json.
    /// Passed directly to Confluent's ProducerConfig / ConsumerConfig.
    /// </summary>
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
    }
}
