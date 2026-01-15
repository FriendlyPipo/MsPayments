using FluentValidation;
using Payments.Application.Commands;

namespace Payments.Application.Validators
{
    public class FailPaymentCommandValidator : AbstractValidator<FailPaymentCommand>
    {
        public FailPaymentCommandValidator()
        {
            RuleFor(x => x.StripeId).NotEmpty().WithMessage("El ID de Stripe no puede estar vacio");
        }
    }
}
