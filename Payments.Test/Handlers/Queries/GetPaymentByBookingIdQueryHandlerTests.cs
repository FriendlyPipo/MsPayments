using FluentAssertions;
using Moq;
using Payments.Application.Handlers.Queries;
using Payments.Application.Queries;
using Payments.Core.Dtos;
using Payments.Core.Repositories;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Handlers.Queries
{
    public class GetPaymentByBookingIdQueryHandlerTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly GetPaymentByBookingIdQueryHandler _handler;

        public GetPaymentByBookingIdQueryHandlerTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _handler = new GetPaymentByBookingIdQueryHandler(_paymentRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnPaymentDto_WhenPaymentExists()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var query = new GetPaymentByBookingIdQuery(bookingId);
            
            var payment = Payment.Create(
                Guid.NewGuid(), bookingId, Guid.NewGuid(), "pi_123", 
                100m, "USD", "test@test.com", "Test User", 
                DateTime.UtcNow, null, "Exitoso"
            );

            _paymentRepositoryMock
                .Setup(r => r.GetPaymentByBookingIdAsync(It.Is<BookingId>(id => id.Value == bookingId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.BookingId.Should().Be(bookingId);
            result.Status.Should().Be("Exitoso");
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenPaymentDoesNotExist()
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(r => r.GetPaymentByBookingIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _handler.Handle(new GetPaymentByBookingIdQuery(Guid.NewGuid()), CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
