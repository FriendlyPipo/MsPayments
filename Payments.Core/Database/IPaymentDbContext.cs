using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;

namespace Payments.Core.Database
{
    public interface IPaymentDbContext
    {
        DbSet<Payment> Payments { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        IPaymentDbContextTransactionProxy BeginTransaction();
    }
}
