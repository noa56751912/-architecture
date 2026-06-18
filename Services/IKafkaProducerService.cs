namespace Services
{
    public interface IKafkaProducerService
    {
        /// <summary>
        /// Publishes an order event to the configured Kafka topic.
        /// </summary>
        Task PublishOrderEventAsync(OrderEventMessage message);
    }
}
