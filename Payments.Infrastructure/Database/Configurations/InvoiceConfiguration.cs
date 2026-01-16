using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payments.Infrastructure.Database.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.InvoiceId);

            builder.Property(i => i.InvoiceId)
                .HasConversion(
                    id => id.Value,
                    value => InvoiceId.Create(value))
                .ValueGeneratedNever();

            builder.Property(i => i.PaymentId)
                .HasConversion(
                    id => id.Value,
                    value => PaymentId.Create(value));

            builder.Property(i => i.UserId)
                .HasConversion(
                    id => id.Value,
                    value => UserId.Create(value));

            builder.Property(i => i.Total)
                .HasConversion(
                    total => total.Value,
                    value => InvoiceTotal.Create(value));

            builder.Property(i => i.Currency)
                .HasConversion(
                    currency => currency.Value,
                    value => InvoiceCurrency.Create(value))
                .HasMaxLength(3);

            builder.Property(i => i.UserName)
                .IsRequired()
                .HasMaxLength(200);

            // Foreign Key Relationship
            builder.HasOne<Payment>()
                .WithMany()
                .HasForeignKey(i => i.PaymentId);
        }
    }
}
