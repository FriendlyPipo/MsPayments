using System.Threading;
using System.Threading.Tasks;

namespace Payments.Core.RabbitMQ.Consumer
{
    public interface IRabbitMQConsumer
    {
        void StartConsuming();
        void StopConsuming();
    }
}
