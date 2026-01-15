using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payments.Core.Database
{
    public interface IPaymentDbContextTransactionProxy : IDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
