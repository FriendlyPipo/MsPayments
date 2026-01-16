using FluentAssertions;
using Moq;
using Payments.Application.Commands;
using Payments.Application.Handlers.Commands;
using Payments.Core.Dtos;
using Payments.Core.RabbitMQ;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Handlers.Commands
{
    public class CreatePaymentCommandHandlerTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IStripeService> _stripeServiceMock;
        private readonly Mock<IEventBus<PaymentNotificationDto>> _notificationBusMock;
        private readonly Mock<IUserLogService> _userLogServiceMock;
        private readonly CreatePaymentCommandHandler _handler;

        public CreatePaymentCommandHandlerTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _stripeServiceMock = new Mock<IStripeService>();
            _notificationBusMock = new Mock<IEventBus<PaymentNotificationDto>>();
            _userLogServiceMock = new Mock<IUserLogService>();

            _handler = new CreatePaymentCommandHandler(
                _paymentRepositoryMock.Object,
                _stripeServiceMock.Object,
                _notificationBusMock.Object,
                _userLogServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnClientSecret_WhenPaymentCreatedSuccessfully()
        {
            // Arrange
            var request = new CreatePaymentCommand
            {
                BookingId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                UserEmail = "test@example.com",
                UserName = "Test User",
                Total = 100.0m,
                Currency = "USD"
            };

            var stripeIntent = new StripePaymentIntentDto
            {
                StripeId = "pi_test",
                ClientSecret = "secret_test"
            };

            _stripeServiceMock
                .Setup(s => s.CreatePaymentIntentAsync(request.Total, request.Currency, request.BookingId.ToString()))
                .ReturnsAsync(stripeIntent);

            _paymentRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _userLogServiceMock
                .Setup(l => l.LogAsync(It.IsAny<UserLogDto>()))
                .Returns(Task.CompletedTask);

            _notificationBusMock
                .Setup(b => b.PublishMessageAsync(It.IsAny<PaymentNotificationDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().Be("secret_test");
            
            _paymentRepositoryMock.Verify(r => r.AddAsync(It.Is<Payment>(p => 
                p.BookingId.Value == request.BookingId && 
                p.UserId.Value == request.UserId &&
                p.UserEmail == request.UserEmail &&
                p.UserName == request.UserName &&
                p.Total.Value == request.Total &&
                p.Currency.Value == request.Currency
            ), It.IsAny<CancellationToken>()), Times.Once);

            _userLogServiceMock.Verify(l => l.LogAsync(It.Is<UserLogDto>(log => 
                log.Title == "Pago Creado" &&
                log.UserId == request.UserId
            )), Times.Once);

            _notificationBusMock.Verify(b => b.PublishMessageAsync(It.Is<PaymentNotificationDto>(n => 
                n.BookingId == request.BookingId &&
                n.Status == "Creado"
            ), "notifications_queue", "PaymentCreated"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenStripeServiceFails()
        {
            // Arrange
            var request = new CreatePaymentCommand { Total = 100, Currency = "USD", BookingId = Guid.NewGuid() };
            
            _stripeServiceMock
                .Setup(s => s.CreatePaymentIntentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Stripe error"));

            // Act
            Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Stripe error");
            _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
