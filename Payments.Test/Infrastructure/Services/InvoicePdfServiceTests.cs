using FluentAssertions;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using Payments.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Infrastructure.Services
{
    public class InvoicePdfServiceTests
    {
        private readonly InvoicePdfService _service;

        public InvoicePdfServiceTests()
        {
            _service = new InvoicePdfService();
        }

        [Fact]
        public async Task GeneratePdfAsync_ShouldReturnByteArray_WhenInvoiceIsValid()
        {
            // Arrange
            var invoice = Invoice.Create(
                InvoiceId.Create(Guid.NewGuid()),
                PaymentId.Create(Guid.NewGuid()),
                UserId.Create(Guid.NewGuid()),
                InvoiceTotal.Create(100m),
                InvoiceCurrency.Create("USD"),
                "Test User",
                "test@example.com"
            );

            // Act
            var result = await _service.GeneratePdfAsync(invoice);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }
    }
}
