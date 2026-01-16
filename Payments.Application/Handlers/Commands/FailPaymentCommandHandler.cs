using MediatR;
using Payments.Application.Commands;
using Payments.Core.Dtos;
using Payments.Core.RabbitMQ;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace Payments.Application.Handlers.Commands
{
    public class FailPaymentCommandHandler : IRequestHandler<FailPaymentCommand, bool>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IEventBus<UpdateBookingStatusEventDto> _eventBus;
        private readonly IEventBus<PaymentNotificationDto> _notificationBus;
        private readonly IUserLogService _userLogService;

        public FailPaymentCommandHandler(
            IPaymentRepository paymentRepository, 
            IEventBus<UpdateBookingStatusEventDto> eventBus,
            IEventBus<PaymentNotificationDto> notificationBus,
            IUserLogService userLogService)
        {
            _paymentRepository = paymentRepository;
            _eventBus = eventBus;
            _notificationBus = notificationBus;
            _userLogService = userLogService;
        }

        public async Task<bool> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetPaymentByStripeIdAsync(
                PaymentStripeId.Create(request.StripeId), 
                cancellationToken);

            if (payment == null) return false;

            payment.UpdateStatus(PaymentStatus.Fallido);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Registrar Log
            await _userLogService.LogAsync(new UserLogDto
            {
                UserId = payment.UserId.Value,
                Title = "Pago Fallido",
                Description = $"El pago con StripeId {request.StripeId} para la reserva {payment.BookingId.Value} ha fallado."
            });

            // Notificar a Bookings que el pago fallo (vuelve a Pendiente o se Cancela)
            var bookingUpdate = new UpdateBookingStatusEventDto
            {
                BookingId = payment.BookingId.Value,
                Status = "Cancelado" 
            };

            await _eventBus.PublishMessageAsync(bookingUpdate, "booking_queue", "UpdateBookingStatus");

            // Notificar al usuario
            var notification = new PaymentNotificationDto
            {
                BookingId = payment.BookingId.Value,
                UserId = payment.UserId.Value,
                UserName = payment.UserName,
                UserEmail = payment.UserEmail,
                Total = payment.Total.Value,
                Currency = payment.Currency.Value,
                Status = "Fallido"
            };

            await _notificationBus.PublishMessageAsync(notification, "notifications_queue", "PaymentFailed");

            return true;
        }
    }
}
