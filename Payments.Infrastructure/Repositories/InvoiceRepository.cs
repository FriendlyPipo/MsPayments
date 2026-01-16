using System.Threading.Tasks;
using Payments.Core.Repositories;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using Payments.Infrastructure.Database;

namespace Payments.Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly PaymentDbContext _context;

        public InvoiceRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task<Invoice?> GetByIdAsync(InvoiceId id)
        {
            return await _context.Invoices.FindAsync(id);
        }
    }
}
