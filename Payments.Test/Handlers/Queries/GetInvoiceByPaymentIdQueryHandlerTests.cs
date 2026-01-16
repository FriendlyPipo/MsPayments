using FluentAssertions;
using Moq;
using Payments.Application.Handlers.Queries;
using Payments.Application.Queries;
using Payments.Core.Repositories;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Handlers.Queries
{
    public class GetInvoiceByPaymentIdQueryHandlerTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly GetInvoiceByPaymentIdQueryHandler _handler;

        public GetInvoiceByPaymentIdQueryHandlerTests()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _handler = new GetInvoiceByPaymentIdQueryHandler(_invoiceRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnInvoiceDto_WhenInvoiceExists()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var invoice = Invoice.Create(
                InvoiceId.Create(Guid.NewGuid()),
                PaymentId.Create(paymentId),
                UserId.Create(Guid.NewGuid()),
                InvoiceTotal.Create(100m),
                InvoiceCurrency.Create("USD"),
                "Test User",
                "test@example.com"
            );

            _invoiceRepositoryMock.Setup(r => r.GetByPaymentIdAsync(It.IsAny<PaymentId>()))
                .ReturnsAsync(invoice);

            var query = new GetInvoiceByPaymentIdQuery(paymentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.PaymentId.Should().Be(paymentId);
            result.UserName.Should().Be("Test User");
            result.Total.Should().Be(100m);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenInvoiceDoesNotExist()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            _invoiceRepositoryMock.Setup(r => r.GetByPaymentIdAsync(It.IsAny<PaymentId>()))
                .ReturnsAsync((Invoice?)null);

            var query = new GetInvoiceByPaymentIdQuery(paymentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
