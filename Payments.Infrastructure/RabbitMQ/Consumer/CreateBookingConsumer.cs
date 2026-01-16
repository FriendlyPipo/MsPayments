using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payments.Application.Commands;
using Payments.Core.RabbitMQ;
using MediatR;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;
using System;

namespace Payments.Infrastructure.RabbitMQ.Consumer
{
    public class CreateBookingConsumer : IRabbitMQConsumer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CreateBookingConsumer> _logger;
        private readonly IConnectionRabbitMQ _connection;

        public CreateBookingConsumer(IServiceProvider serviceProvider, ILogger<CreateBookingConsumer> logger, IConnectionRabbitMQ connection)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _connection = connection;
        }

        public async Task ConsumeMessagesAsync(string queueName)
        {
            var channel = _connection.GetChannel();
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var eventMessage = JsonConvert.DeserializeObject<EventMessage<object>>(message);

                if (eventMessage?.EventType == "CreateBooking")
                {
                    _logger.LogInformation($"Evento CreateBooking recibido: {eventMessage.Data}");
                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var bookingData = JsonConvert.DeserializeObject<dynamic>(eventMessage.Data.ToString()!);
                        
                        var command = new CreatePaymentCommand
                        {
                            BookingId = (Guid)bookingData.BookingId,
                            UserId = (Guid)bookingData.UserId,
                            UserEmail = (string)bookingData.UserEmail,
                            UserName = (string)bookingData.UserName ?? "Usuario", 
                            Total = (decimal)bookingData.Total,
                            Currency = "USD" 
                        };

                        await mediator.Send(command);
                        _logger.LogInformation($"Pago iniciado para BookingId: {command.BookingId}");
                    }
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        }
    }
}
