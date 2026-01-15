using Payments.Core.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Payments.Infrastructure.RabbitMQ.Connection
{
    public class RabbitMQBackgroundService : BackgroundService
    {
        private readonly IRabbitMQConsumer _rabbitMQConsumer;
        private readonly ILogger<RabbitMQBackgroundService> _logger;

        public RabbitMQBackgroundService(IRabbitMQConsumer rabbitMQConsumer, ILogger<RabbitMQBackgroundService> logger)
        {
            _rabbitMQConsumer = rabbitMQConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Esperando la inicializacion de Rabbit en Payments");

            await Task.Delay(3000, stoppingToken); 
            await _rabbitMQConsumer.ConsumeMessagesAsync("payment_queue");

            _logger.LogInformation("Consumidores de Rabbit iniciados en Payments.");
        }
    }
}
