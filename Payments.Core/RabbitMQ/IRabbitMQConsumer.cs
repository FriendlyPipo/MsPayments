using System.Threading;
using System.Threading.Tasks;

namespace Payments.Core.RabbitMQ
{
    public interface IRabbitMQConsumer
    {
        Task ConsumeMessagesAsync(string queueName);
    }
}
