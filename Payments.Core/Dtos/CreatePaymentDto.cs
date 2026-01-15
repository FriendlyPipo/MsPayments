using System;

namespace Payments.Core.Dtos
{
    public class CreatePaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string StripeId { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
