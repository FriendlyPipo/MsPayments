using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Payments.Core.Repositories;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using Payments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Payments.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            await _context.Payments.AddAsync(payment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(BookingId bookingId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
        }

        public async Task<Payment?> GetPaymentByStripeIdAsync(PaymentStripeId stripeId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.StripeId == stripeId, cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pendiente)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
