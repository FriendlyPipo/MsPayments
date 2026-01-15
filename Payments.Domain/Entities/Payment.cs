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
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public PaymentStatus Status { get; private set; }

        private Payment(PaymentId paymentId, BookingId bookingId, UserId userId, PaymentStripeId stripeId, PaymentTotal total, DateTime createdAt, DateTime? updatedAt, PaymentStatus status)
        {
            PaymentId = paymentId;
            BookingId = bookingId;
            UserId = userId;
            StripeId = stripeId;
            Total = total;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Status = status;
        }

        public static Payment Create(Guid paymentId, Guid bookingId, Guid userId, string stripeId, decimal total, DateTime createdAt, DateTime? updatedAt, string status)
        {
            return new Payment(
                PaymentId.Create(paymentId),
                BookingId.Create(bookingId),
                UserId.Create(userId),
                PaymentStripeId.Create(stripeId),
                PaymentTotal.Create(total),
                createdAt,
                updatedAt,
                status
            );
        }

        public void UpdateStatus(PaymentStatus status)
        {
            Status = status;
        }
    }   
}
