using MediatR;
using System;

namespace Payments.Application.Commands
{
    public class CreatePaymentCommand : IRequest<string>
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "USD";
    }
}
