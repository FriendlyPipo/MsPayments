using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payments.Core.RabbitMQ;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.ValueObjects;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;
using System;

namespace Payments.Infrastructure.RabbitMQ.Consumer
{
    public class UpdateBookingStatusConsumer : IRabbitMQConsumer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UpdateBookingStatusConsumer> _logger;
        private readonly IConnectionRabbitMQ _connection;

        public UpdateBookingStatusConsumer(IServiceProvider serviceProvider, ILogger<UpdateBookingStatusConsumer> logger, IConnectionRabbitMQ connection)
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

                if (eventMessage?.EventType == "UpdateBookingStatus")
                {
                    var statusData = JsonConvert.DeserializeObject<dynamic>(eventMessage.Data.ToString()!);
                    string newStatus = statusData.Status;
                    Guid bookingId = statusData.BookingId;

                    if (newStatus == "Cancelado")
                    {
                        _logger.LogInformation($"Cancelando pago para reserva con ID: {bookingId}");
                        
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var paymentRepo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
                            var stripeService = scope.ServiceProvider.GetRequiredService<IStripeService>();

                            var payment = await paymentRepo.GetPaymentByBookingIdAsync(BookingId.Create(bookingId));
                            if (payment != null && payment.Status == Domain.Entities.PaymentStatus.Pendiente)
                            {
                                await stripeService.CancelPaymentIntentAsync(payment.StripeId.Value);
                                payment.Cancel();
                                await paymentRepo.UpdateAsync(payment);
                                
                                _logger.LogInformation($"Pago cancelado exitosamente para reserva con ID: {bookingId}");
                            }
                        }
                    }
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        }
    }
}
