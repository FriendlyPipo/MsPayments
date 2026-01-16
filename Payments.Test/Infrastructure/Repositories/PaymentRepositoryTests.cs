using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Infrastructure.Repositories
{
    public class PaymentRepositoryTests
    {
        private readonly PaymentDbContext _context;
        private readonly PaymentRepository _repository;

        public PaymentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PaymentDbContext(options);
            _repository = new PaymentRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPaymentToDatabase()
        {
            // Arrange
            var payment = Payment.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "pi_test",
                100m, "USD", "test@test.com", "Test User",
                DateTime.UtcNow, null, "Pendiente"
            );

            // Act
            await _repository.AddAsync(payment);

            // Assert
            var savedPayment = await _context.Payments.FindAsync(payment.PaymentId);
            savedPayment.Should().NotBeNull();
            savedPayment!.StripeId.Value.Should().Be("pi_test");
        }

        [Fact]
        public async Task GetPaymentByBookingIdAsync_ShouldReturnCorrectPayment()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var payment = Payment.Create(
                Guid.NewGuid(), bookingId, Guid.NewGuid(), "pi_test",
                100m, "USD", "test@test.com", "Test User",
                DateTime.UtcNow, null, "Pendiente"
            );
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPaymentByBookingIdAsync(BookingId.Create(bookingId));

            // Assert
            result.Should().NotBeNull();
            result!.BookingId.Value.Should().Be(bookingId);
        }

        [Fact]
        public async Task GetPaymentByStripeIdAsync_ShouldReturnCorrectPayment()
        {
            // Arrange
            var stripeId = "pi_stripe_123";
            var payment = Payment.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), stripeId,
                100m, "USD", "test@test.com", "Test User",
                DateTime.UtcNow, null, "Pendiente"
            );
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPaymentByStripeIdAsync(PaymentStripeId.Create(stripeId));

            // Assert
            result.Should().NotBeNull();
            result!.StripeId.Value.Should().Be(stripeId);
        }

        [Fact]
        public async Task GetPendingPaymentsAsync_ShouldReturnOnlyPendingPayments()
        {
            // Arrange
            var pendingPayment = Payment.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "pi_pending",
                100m, "USD", "test@test.com", "Test User",
                DateTime.UtcNow, null, "Pendiente"
            );
            var successfulPayment = Payment.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "pi_success",
                100m, "USD", "test@test.com", "Test User",
                DateTime.UtcNow, null, "Exitoso"
            );

            await _context.Payments.AddRangeAsync(pendingPayment, successfulPayment);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetPendingPaymentsAsync();

            // Assert
            results.Should().HaveCount(1);
            results.First().StripeId.Value.Should().Be("pi_pending");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePaymentInDatabase()
        {
            // Arrange
            var payment = Payment.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "pi_test",
                100m, "USD", "test@test.com", "Test User",
                DateTime.UtcNow, null, "Pendiente"
            );
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // Act
            payment.UpdateStatus(PaymentStatus.Exitoso);
            await _repository.UpdateAsync(payment);

            // Assert
            var updatedPayment = await _context.Payments.FindAsync(payment.PaymentId);
            updatedPayment!.Status.Should().Be(PaymentStatus.Exitoso);
        }
    }
}
