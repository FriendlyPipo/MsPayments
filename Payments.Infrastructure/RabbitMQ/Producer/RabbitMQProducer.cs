using System.Text;
using RabbitMQ.Client;
using Payments.Core.RabbitMQ;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Payments.Infrastructure.RabbitMQ.Producer
{
    public class RabbitMQProducer<T> : IEventBus<T>
    {
        private readonly IConnectionRabbitMQ _rabbitMQConnection;
        private readonly ILogger<RabbitMQProducer<T>> _logger;

        public RabbitMQProducer(IConnectionRabbitMQ rabbitMQConnection, ILogger<RabbitMQProducer<T>> logger)
        {
            _rabbitMQConnection = rabbitMQConnection;
            _logger = logger;
        }

        public async Task PublishMessageAsync(T data, string queueName, string eventType)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "El mensaje no puede ser nulo.");
            }

            var channel = _rabbitMQConnection.GetChannel();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            var eventMessage = new
            {
                EventType = eventType,
                Data = data
            };

            var messageBody = JsonConvert.SerializeObject(eventMessage);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var basicProperties = new BasicProperties
            {
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync<BasicProperties>(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: basicProperties,
                body: body,
                cancellationToken: CancellationToken.None
            );

            _logger.LogInformation($"Mensaje publicado en '{queueName}' con evento '{eventType}': {messageBody}");
        }
    }
}
