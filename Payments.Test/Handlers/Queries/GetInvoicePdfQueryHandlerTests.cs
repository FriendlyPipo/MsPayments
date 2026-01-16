using FluentAssertions;
using Moq;
using Payments.Application.Handlers.Queries;
using Payments.Application.Queries;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Handlers.Queries
{
    public class GetInvoicePdfQueryHandlerTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly Mock<IPdfService<Invoice>> _pdfServiceMock;
        private readonly GetInvoicePdfQueryHandler _handler;

        public GetInvoicePdfQueryHandlerTests()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _pdfServiceMock = new Mock<IPdfService<Invoice>>();
            _handler = new GetInvoicePdfQueryHandler(_invoiceRepositoryMock.Object, _pdfServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnPdfBytes_WhenInvoiceExists()
        {
            // Arrange
            var invoiceId = Guid.NewGuid();
            var query = new GetInvoicePdfQuery(invoiceId);
            
            var invoice = Invoice.Create(
                InvoiceId.Create(invoiceId),
                PaymentId.Create(Guid.NewGuid()),
                UserId.Create(Guid.NewGuid()),
                InvoiceTotal.Create(100m),
                InvoiceCurrency.Create("USD"),
                "User",
                "user@test.com"
            );

            var pdfBytes = new byte[] { 1, 2, 3 };

            _invoiceRepositoryMock
                .Setup(r => r.GetByIdAsync(It.Is<InvoiceId>(id => id.Value == invoiceId)))
                .ReturnsAsync(invoice);

            _pdfServiceMock
                .Setup(s => s.GeneratePdfAsync(invoice))
                .ReturnsAsync(pdfBytes);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(pdfBytes);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenInvoiceNotFound()
        {
            // Arrange
            _invoiceRepositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<InvoiceId>()))
                .ReturnsAsync((Invoice?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(new GetInvoicePdfQuery(Guid.NewGuid()), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Factura no encontrada");
        }
    }
}
