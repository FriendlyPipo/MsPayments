using RabbitMQ.Client;
using System.Threading.Tasks;

namespace Payments.Core.RabbitMQ
{
    public interface IConnectionRabbitMQ
    {
        Task InitializeAsync();
        IChannel GetChannel();
    }
}
