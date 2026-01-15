using MediatR;
using System;

namespace Payments.Application.Commands
{
    public class CapturePaymentCommand : IRequest<bool>
    {
        public string StripeId { get; set; } = string.Empty;
    }
}
