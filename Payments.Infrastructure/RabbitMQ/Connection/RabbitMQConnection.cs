using Payments.Core.RabbitMQ;
using RabbitMQ.Client;
using Payments.Core.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Payments.Infrastructure.RabbitMQ.Connection
{
    public class RabbitMQConnection : IConnectionRabbitMQ
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly IConnectionFactory _connectionFactory;

        public RabbitMQConnection(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync()
        {
            try 
            {
                _connection = await _connectionFactory.CreateConnectionAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new RabbitMQException("Error al conectar con el servidor RabbitMQ.", ex);
            }

            if (_connection == null)
            {
                throw new RabbitMQException("No se pudo establecer la conexion con RabbitMQ.");
            }

            _channel = await _connection.CreateChannelAsync();

            if (_channel == null)
            {
                throw new RabbitMQException("No se pudo crear el canal de comunicacion con RabbitMQ.");
            }

            await _channel.QueueDeclareAsync("payment_queue", true, false, false);
        }

        public IChannel GetChannel()
        {
            if (_channel == null)
            {
                throw new RabbitMQException("RabbitMQ aun no esta inicializado correctamente.");
            }
            return _channel;
        }
    }
}