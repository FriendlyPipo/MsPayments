using System;

namespace Payments.Core.Dtos
{
    public class GetPaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string StripeId { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
