using Payments.Core.Database;
using Payments.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;
using System;
using EntityFramework.Exceptions.PostgreSQL; 
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Database
{
    public class PaymentDbContext : DbContext, IPaymentDbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        public DbContext DbContext => this;

        public IPaymentDbContextTransactionProxy BeginTransaction()
        {
            return new PaymentDbContextTransactionProxy(this);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseExceptionProcessor();
        }

        public virtual void SetPropertyIsModifiedToFalse<TEntity, TProperty>(
            TEntity entity,
            Expression<Func<TEntity, TProperty>> propertyExpression
        )
           where TEntity : class
        {
            Entry(entity).Property(propertyExpression).IsModified = false;
        }

        public virtual void ChangeEntityState<TEntity>(TEntity entity, EntityState state)
        {
            if (entity != null)
            {
                Entry(entity).State = state;
            }
        }

        public async Task<bool> SaveEfContextChanges(CancellationToken cancellationToken = default)
        {
            return await SaveChangesAsync(cancellationToken) >= 0;
        }
    }
}