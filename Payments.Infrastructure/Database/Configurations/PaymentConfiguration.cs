using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Payments.Infrastructure.Database.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.PaymentId)
                .HasConversion(
                    id => id.Value,
                    value => PaymentId.Create(value))
                .ValueGeneratedNever();

            builder.Property(p => p.BookingId)
                .HasConversion(
                    id => id.Value,
                    value => BookingId.Create(value));

            builder.Property(p => p.UserId)
                .HasConversion(
                    id => id.Value,
                    value => UserId.Create(value));

            builder.Property(p => p.StripeId)
                .HasConversion(
                    id => id.Value,
                    value => PaymentStripeId.Create(value));

            builder.Property(p => p.Total)
                .HasConversion(
                    total => total.Value,
                    value => PaymentTotal.Create(value));

            builder.Property(p => p.Status)
                .HasConversion<string>();
        }
    }
}
