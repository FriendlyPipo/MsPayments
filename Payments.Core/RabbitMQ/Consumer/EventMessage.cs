namespace Payments.Core.RabbitMQ.Consumer
{
    public class EventMessage
    {
        public string EventId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public object Data { get; set; } = new { };
        public string Timestamp { get; set; } = string.Empty;
    }
}
