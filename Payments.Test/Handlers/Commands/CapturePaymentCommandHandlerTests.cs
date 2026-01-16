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
    public class CapturePaymentCommandHandlerTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IEventBus<UpdateBookingStatusEventDto>> _eventBusMock;
        private readonly Mock<IEventBus<PaymentNotificationDto>> _notificationBusMock;
        private readonly Mock<IPdfService<Invoice>> _pdfServiceMock;
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly Mock<IUserLogService> _userLogServiceMock;
        private readonly CapturePaymentCommandHandler _handler;

        public CapturePaymentCommandHandlerTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _eventBusMock = new Mock<IEventBus<UpdateBookingStatusEventDto>>();
            _notificationBusMock = new Mock<IEventBus<PaymentNotificationDto>>();
            _pdfServiceMock = new Mock<IPdfService<Invoice>>();
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _userLogServiceMock = new Mock<IUserLogService>();

            _handler = new CapturePaymentCommandHandler(
                _paymentRepositoryMock.Object,
                _eventBusMock.Object,
                _notificationBusMock.Object,
                _pdfServiceMock.Object,
                _invoiceRepositoryMock.Object,
                _userLogServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnTrueAndProcessEverything_WhenPaymentExists()
        {
            // Arrange
            var stripeId = "pi_test";
            var request = new CapturePaymentCommand { StripeId = stripeId };
            
            var payment = Payment.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), stripeId, 
                150.0m, "USD", "test@test.com", "Tester", 
                DateTime.UtcNow, null, "Pendiente"
            );

            _paymentRepositoryMock
                .Setup(r => r.GetPaymentByStripeIdAsync(It.Is<PaymentStripeId>(s => s.Value == stripeId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            _paymentRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _invoiceRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Invoice>()))
                .Returns(Task.CompletedTask);

            _pdfServiceMock
                .Setup(s => s.GeneratePdfAsync(It.IsAny<Invoice>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            _eventBusMock
                .Setup(b => b.PublishMessageAsync(It.IsAny<UpdateBookingStatusEventDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _notificationBusMock
                .Setup(b => b.PublishMessageAsync(It.IsAny<PaymentNotificationDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _userLogServiceMock
                .Setup(l => l.LogAsync(It.IsAny<UserLogDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            payment.Status.Should().Be(PaymentStatus.Exitoso);

            _paymentRepositoryMock.Verify(r => r.UpdateAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
            _invoiceRepositoryMock.Verify(r => r.CreateAsync(It.Is<Invoice>(i => i.PaymentId == payment.PaymentId)), Times.Once);
            _pdfServiceMock.Verify(s => s.GeneratePdfAsync(It.IsAny<Invoice>()), Times.Once);
            
            _eventBusMock.Verify(b => b.PublishMessageAsync(It.Is<UpdateBookingStatusEventDto>(d => 
                d.BookingId == payment.BookingId.Value && d.Status == "Confirmado"
            ), "booking_queue", "UpdateBookingStatus"), Times.Once);

            _userLogServiceMock.Verify(l => l.LogAsync(It.Is<UserLogDto>(log => 
                log.Title == "Pago Confirmado"
            )), Times.Once);

            _notificationBusMock.Verify(b => b.PublishMessageAsync(It.Is<PaymentNotificationDto>(n => 
                n.Status == "Confirmado" && n.InvoicePdf != null
            ), "notifications_queue", "PaymentConfirmed"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenPaymentNotFound()
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(r => r.GetPaymentByStripeIdAsync(It.IsAny<PaymentStripeId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _handler.Handle(new CapturePaymentCommand { StripeId = "unknown" }, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
