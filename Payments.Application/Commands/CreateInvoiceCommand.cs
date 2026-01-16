using MediatR;
using System;

namespace Payments.Application.Commands
{
    public class CreateInvoiceCommand : IRequest<bool>
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
