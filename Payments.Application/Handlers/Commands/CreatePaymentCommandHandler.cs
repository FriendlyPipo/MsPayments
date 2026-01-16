using MediatR;
using Payments.Application.Commands;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System;
using Payments.Core.Dtos;
using Payments.Core.RabbitMQ;

namespace Payments.Application.Handlers.Commands
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, string>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IStripeService _stripeService;
        private readonly IEventBus<PaymentNotificationDto> _notificationBus;
        private readonly IUserLogService _userLogService;

        public CreatePaymentCommandHandler(
            IPaymentRepository paymentRepository, 
            IStripeService stripeService,
            IEventBus<PaymentNotificationDto> notificationBus,
            IUserLogService userLogService)
        {
            _paymentRepository = paymentRepository;
            _stripeService = stripeService;
            _notificationBus = notificationBus;
            _userLogService = userLogService;
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
                request.UserEmail,
                request.UserName,
                DateTime.UtcNow,
                null,
                "Pendiente"
            );

            await _paymentRepository.AddAsync(payment, cancellationToken);

            // Registrar Log
            await _userLogService.LogAsync(new UserLogDto
            {
                UserId = payment.UserId.Value,
                Title = "Pago Creado",
                Description = $"Se ha iniciado un proceso de pago para la reserva {payment.BookingId.Value} por un total de {payment.Total.Value} {payment.Currency.Value}."
            });

            // Notificar que el pago fue iniciado
            var notification = new PaymentNotificationDto
            {
                BookingId = payment.BookingId.Value,
                UserId = payment.UserId.Value,
                UserName = payment.UserName,
                UserEmail = payment.UserEmail,
                Total = payment.Total.Value,
                Currency = payment.Currency.Value,
                Status = "Creado"
            };

            await _notificationBus.PublishMessageAsync(notification, "notifications_queue", "PaymentCreated");

            // Retornar el ClientSecret para que el frontend complete el pago
            return stripeIntent.ClientSecret;
        }
    }
}
