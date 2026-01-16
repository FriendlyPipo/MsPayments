using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Payments.Core.RabbitMQ;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using Payments.Infrastructure.RabbitMQ.Consumer;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Infrastructure.RabbitMQ
{
    public class UpdateBookingStatusConsumerTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ILogger<UpdateBookingStatusConsumer>> _loggerMock;
        private readonly Mock<IConnectionRabbitMQ> _connectionMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IStripeService> _stripeServiceMock;
        private readonly UpdateBookingStatusConsumer _consumer;

        public UpdateBookingStatusConsumerTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _loggerMock = new Mock<ILogger<UpdateBookingStatusConsumer>>();
            _connectionMock = new Mock<IConnectionRabbitMQ>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _stripeServiceMock = new Mock<IStripeService>();

            // Setup Scope
            var serviceScopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);
            
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);

            scopeServiceProviderMock.Setup(x => x.GetService(typeof(IPaymentRepository)))
                .Returns(_paymentRepositoryMock.Object);
            scopeServiceProviderMock.Setup(x => x.GetService(typeof(IStripeService)))
                .Returns(_stripeServiceMock.Object);

            _consumer = new UpdateBookingStatusConsumer(_serviceProviderMock.Object, _loggerMock.Object, _connectionMock.Object);
        }

        [Fact]
        public async Task ProcessEventAsync_ShouldCancelPayment_WhenStatusIsCancelado()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var statusData = new { Status = "Cancelado", BookingId = bookingId };
            var eventMessage = new EventMessage<object>
            {
                EventType = "UpdateBookingStatus",
                Data = JsonConvert.SerializeObject(statusData)
            };

            var payment = Payment.Create(
                Guid.NewGuid(), bookingId, Guid.NewGuid(), "pi_test",
                100m, "USD", "test@test.com", "User",
                DateTime.UtcNow, null, "Pendiente"
            );

            _paymentRepositoryMock.Setup(r => r.GetPaymentByBookingIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            await _consumer.ProcessEventAsync(eventMessage);

            // Assert
            _stripeServiceMock.Verify(s => s.CancelPaymentIntentAsync(payment.StripeId.Value), Times.Once);
            _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Payment>(p => p.Status == PaymentStatus.Fallido), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
