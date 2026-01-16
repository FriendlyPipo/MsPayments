using System;

namespace Payments.Core.Dtos
{
    public class CreateInvoiceDto
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
