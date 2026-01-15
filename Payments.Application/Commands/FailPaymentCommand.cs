using MediatR;
using System;

namespace Payments.Application.Commands
{
    public class FailPaymentCommand : IRequest<bool>
    {
        public string StripeId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
