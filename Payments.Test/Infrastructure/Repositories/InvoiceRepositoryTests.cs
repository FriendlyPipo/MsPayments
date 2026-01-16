using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Infrastructure.Repositories
{
    public class InvoiceRepositoryTests
    {
        private readonly PaymentDbContext _context;
        private readonly InvoiceRepository _repository;

        public InvoiceRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PaymentDbContext(options);
            _repository = new InvoiceRepository(_context);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddInvoiceToDatabase()
        {
            // Arrange
            var invoice = Invoice.Create(
                InvoiceId.Create(Guid.NewGuid()),
                PaymentId.Create(Guid.NewGuid()),
                UserId.Create(Guid.NewGuid()),
                InvoiceTotal.Create(150m),
                InvoiceCurrency.Create("USD"),
                "Tester",
                "test@test.com"
            );

            // Act
            await _repository.CreateAsync(invoice);

            // Assert
            var savedInvoice = await _context.Invoices.FindAsync(invoice.InvoiceId);
            savedInvoice.Should().NotBeNull();
            savedInvoice!.UserName.Should().Be("Tester");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectInvoice()
        {
            // Arrange
            var invoiceId = Guid.NewGuid();
            var invoice = Invoice.Create(
                InvoiceId.Create(invoiceId),
                PaymentId.Create(Guid.NewGuid()),
                UserId.Create(Guid.NewGuid()),
                InvoiceTotal.Create(150m),
                InvoiceCurrency.Create("USD"),
                "Tester",
                "test@test.com"
            );
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(InvoiceId.Create(invoiceId));

            // Assert
            result.Should().NotBeNull();
            result!.InvoiceId.Value.Should().Be(invoiceId);
        }
    }
}
