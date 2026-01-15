using System.Diagnostics.CodeAnalysis;

namespace Payments.Infrastructure.RabbitMQ.Consumer
{
    [ExcludeFromCodeCoverage]
    public class EventMessage<T>
    {
        public string EventType { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
    }
}
