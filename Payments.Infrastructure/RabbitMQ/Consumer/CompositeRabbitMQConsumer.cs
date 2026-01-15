using Payments.Core.RabbitMQ;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Payments.Infrastructure.RabbitMQ.Consumer
{
    public class CompositeRabbitMQConsumer : IRabbitMQConsumer
    {
        private readonly IEnumerable<IRabbitMQConsumer> _consumers;

        public CompositeRabbitMQConsumer(IEnumerable<IRabbitMQConsumer> consumers)
        {
            _consumers = consumers;
        }

        public async Task ConsumeMessagesAsync(string queueName)
        {
            var tasks = _consumers.Select(c => c.ConsumeMessagesAsync(queueName));
            await Task.WhenAll(tasks);
        }
    }
}
