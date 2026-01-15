using System.Threading.Tasks;

namespace Payments.Core.RabbitMQ
{
    public interface IEventBus<T>
    {
        Task PublishMessageAsync(T data, string queueName, string eventType);
    }
}
