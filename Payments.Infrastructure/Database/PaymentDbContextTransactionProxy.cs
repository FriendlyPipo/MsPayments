using Payments.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Database
{
    public class PaymentDbContextTransactionProxy : IPaymentDbContextTransactionProxy
    {
        private readonly IDbContextTransaction _transaction;

        public PaymentDbContextTransactionProxy(DbContext context)
        {
            _transaction = context.Database.BeginTransaction();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }

        public void Dispose()
        {
            _transaction.Dispose();
        }
    }
}