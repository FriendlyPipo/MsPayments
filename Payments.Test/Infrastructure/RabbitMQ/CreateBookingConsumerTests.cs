using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Payments.Application.Commands;
using Payments.Core.RabbitMQ;
using Payments.Infrastructure.RabbitMQ.Consumer;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Infrastructure.RabbitMQ
{
    public class CreateBookingConsumerTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ILogger<CreateBookingConsumer>> _loggerMock;
        private readonly Mock<IConnectionRabbitMQ> _connectionMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly CreateBookingConsumer _consumer;

        public CreateBookingConsumerTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _loggerMock = new Mock<ILogger<CreateBookingConsumer>>();
            _connectionMock = new Mock<IConnectionRabbitMQ>();
            _mediatorMock = new Mock<IMediator>();

            // Setup Scope
            var serviceScopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);
            
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);

            scopeServiceProviderMock.Setup(x => x.GetService(typeof(IMediator)))
                .Returns(_mediatorMock.Object);

            _consumer = new CreateBookingConsumer(_serviceProviderMock.Object, _loggerMock.Object, _connectionMock.Object);
        }

        [Fact]
        public async Task ProcessEventAsync_ShouldSendCreatePaymentCommand_WhenEventTypeIsCreateBooking()
        {
            // Arrange
            var bookingData = new
            {
                BookingId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                UserEmail = "test@example.com",
                UserName = "Test User",
                Total = 150.0m
            };

            var eventMessage = new EventMessage<object>
            {
                EventType = "CreateBooking",
                Data = JsonConvert.SerializeObject(bookingData)
            };

            // Act
            await _consumer.ProcessEventAsync(eventMessage);

            // Assert
            _mediatorMock.Verify(x => x.Send(It.Is<CreatePaymentCommand>(c => 
                c.BookingId == bookingData.BookingId &&
                c.UserId == bookingData.UserId &&
                c.Total == bookingData.Total
            ), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
