using System;
using System.Threading;
using System.Threading.Tasks;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;

namespace Payments.Core.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetPaymentByStripeIdAsync(PaymentStripeId stripeId, CancellationToken cancellationToken = default);
        Task<Payment?> GetPaymentByBookingIdAsync(BookingId bookingId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
        Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
