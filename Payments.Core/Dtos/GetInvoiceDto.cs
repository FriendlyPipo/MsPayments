using System;

namespace Payments.Core.Dtos
{
    public class GetInvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
