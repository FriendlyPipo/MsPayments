using MediatR;
using Payments.Application.Commands;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Payments.Application.Handlers.Commands
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, string>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IStripeService _stripeService;

        public CreatePaymentCommandHandler(IPaymentRepository paymentRepository, IStripeService stripeService)
        {
            _paymentRepository = paymentRepository;
            _stripeService = stripeService;
        }

        public async Task<string> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            // PaymentIntent en Stripe
            var stripeIntent = await _stripeService.CreatePaymentIntentAsync(
                request.Total, 
                request.Currency, 
                request.BookingId.ToString());

            // Crear el registro de pago en base de datos
            var payment = Payment.Create(
                Guid.NewGuid(),
                request.BookingId,
                request.UserId,
                stripeIntent.StripeId,
                request.Total,
                request.Currency,
                DateTime.UtcNow,
                null,
                "Pendiente"
            );

            await _paymentRepository.AddAsync(payment, cancellationToken);

            // Retornar el ClientSecret para que el frontend complete el pago
            return stripeIntent.ClientSecret;
        }
    }
}
