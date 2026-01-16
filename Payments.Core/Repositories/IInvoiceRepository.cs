using System.Threading.Tasks;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;

namespace Payments.Core.Repositories
{
    public interface IInvoiceRepository
    {
        Task CreateAsync(Invoice invoice);
        Task<Invoice?> GetByIdAsync(InvoiceId id);
        Task<Invoice?> GetByPaymentIdAsync(PaymentId id);
    }
}
