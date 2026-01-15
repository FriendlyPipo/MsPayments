using System;   
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Entities
{
    public class Payment
    {
        public PaymentId PaymentId { get; private set; }
        public BookingId BookingId { get; private set; }
        public UserId UserId { get; private set; }
        public PaymentStripeId StripeId { get; private set; }
        public PaymentTotal Total { get; private set; }
        public PaymentCurrency Currency { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public PaymentStatus Status { get; private set; }

        private Payment(PaymentId paymentId, BookingId bookingId, UserId userId, PaymentStripeId stripeId, PaymentTotal total, PaymentCurrency currency, DateTime createdAt, DateTime? updatedAt, PaymentStatus status)
        {
            PaymentId = paymentId;
            BookingId = bookingId;
            UserId = userId;
            StripeId = stripeId;
            Total = total;
            Currency = currency;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Status = status;
        }

        public static Payment Create(Guid paymentId, Guid bookingId, Guid userId, string stripeId, decimal total, string currency, DateTime createdAt, DateTime? updatedAt, string status)
        {
            if (!Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            {
                paymentStatus = PaymentStatus.Pendiente;
            }

            return new Payment(
                PaymentId.Create(paymentId),
                BookingId.Create(bookingId),
                UserId.Create(userId),
                PaymentStripeId.Create(stripeId),
                PaymentTotal.Create(total),
                PaymentCurrency.Create(currency),
                createdAt,
                updatedAt,
                paymentStatus
            );
        }

        public void UpdateStatus(PaymentStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = PaymentStatus.Fallido; // O podriamos añadir 'Cancelado' al enum si prefieres
            UpdatedAt = DateTime.UtcNow;
        }
    }   
}
