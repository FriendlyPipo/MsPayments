using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Payments.Core.RabbitMQ.Connection
{
    public interface IRabbitMQConnection
    {
        IConnection Connection { get; }
        IChannel GetChannel();
    }
}
