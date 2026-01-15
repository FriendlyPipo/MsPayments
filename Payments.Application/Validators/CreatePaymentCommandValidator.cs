using FluentValidation;
using Payments.Application.Commands;

namespace Payments.Application.Validators
{
    public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
    {
        public CreatePaymentCommandValidator()
        {
            RuleFor(x => x.BookingId).NotEmpty().WithMessage("El ID de reserva no puede estar vacio");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("El ID de usuario no puede estar vacio");
            RuleFor(x => x.Total).GreaterThan(0).WithMessage("El total debe ser mayor a 0");
            RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("La moneda debe ser un codigo de 3 caracteres");
        }
    }
}
