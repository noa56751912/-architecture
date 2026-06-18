namespace Services
{
    /// <summary>
    /// Payload sent to Kafka whenever an order is placed successfully.
    /// </summary>
    public class OrderEventMessage
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public double TotalSum { get; set; }
        public DateOnly OrderDate { get; set; }
        public int ItemCount { get; set; }
        public string EventType { get; set; } = "ORDER_PLACED";
    }
}
