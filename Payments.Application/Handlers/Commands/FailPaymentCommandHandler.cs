using MediatR;
using Payments.Application.Commands;
using Payments.Core.Dtos;
using Payments.Core.RabbitMQ;
using Payments.Core.Repositories;
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

        public FailPaymentCommandHandler(IPaymentRepository paymentRepository, IEventBus<UpdateBookingStatusEventDto> eventBus)
        {
            _paymentRepository = paymentRepository;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetPaymentByStripeIdAsync(
                PaymentStripeId.Create(request.StripeId), 
                cancellationToken);

            if (payment == null) return false;

            payment.UpdateStatus(PaymentStatus.Fallido);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Notificar a Bookings que el pago fallo (vuelve a Pendiente o se Cancela)
            var bookingUpdate = new UpdateBookingStatusEventDto
            {
                BookingId = payment.BookingId.Value,
                Status = "Cancelado" 
            };

            await _eventBus.PublishMessageAsync(bookingUpdate, "booking_queue", "UpdateBookingStatus");

            return true;
        }
    }
}
