using FluentValidation;
using Payments.Application.Commands;

namespace Payments.Application.Validators
{
    public class CapturePaymentCommandValidator : AbstractValidator<CapturePaymentCommand>
    {
        public CapturePaymentCommandValidator()
        {
            RuleFor(x => x.StripeId).NotEmpty().WithMessage("El ID de Stripe no puede estar vacio");
        }
    }
}
