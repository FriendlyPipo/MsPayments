using MediatR;
using Payments.Application.Commands;
using Payments.Core.Dtos;
using Payments.Core.RabbitMQ;
using Payments.Core.Repositories;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
using Payments.Core.Services;
using Payments.Domain.ValueObjects;
using System;

namespace Payments.Application.Handlers.Commands
{
    public class CapturePaymentCommandHandler : IRequestHandler<CapturePaymentCommand, bool>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IEventBus<UpdateBookingStatusEventDto> _eventBus;
        private readonly IEventBus<PaymentNotificationDto> _notificationBus;
        private readonly IPdfService<Invoice> _pdfService;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserLogService _userLogService;

        public CapturePaymentCommandHandler(
            IPaymentRepository paymentRepository, 
            IEventBus<UpdateBookingStatusEventDto> eventBus,
            IEventBus<PaymentNotificationDto> notificationBus,
            IPdfService<Invoice> pdfService,
            IInvoiceRepository invoiceRepository,
            IUserLogService userLogService)
        {
            _paymentRepository = paymentRepository;
            _eventBus = eventBus;
            _notificationBus = notificationBus;
            _pdfService = pdfService;
            _invoiceRepository = invoiceRepository;
            _userLogService = userLogService;
        }

        public async Task<bool> Handle(CapturePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetPaymentByStripeIdAsync(
                PaymentStripeId.Create(request.StripeId), 
                cancellationToken);

            if (payment == null) return false;

            payment.UpdateStatus(PaymentStatus.Exitoso);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Crear Factura
            var invoice = Invoice.Create(
                InvoiceId.Create(Guid.NewGuid()),
                payment.PaymentId,
                payment.UserId,
                InvoiceTotal.Create(payment.Total.Value),
                InvoiceCurrency.Create(payment.Currency.Value),
                payment.UserName,
                payment.UserEmail
            );

            await _invoiceRepository.CreateAsync(invoice);

            // Generar PDF
            var pdfBytes = await _pdfService.GeneratePdfAsync(invoice);

            // Notificar a Bookings que el pago fue exitoso
            var bookingUpdate = new UpdateBookingStatusEventDto
            {
                BookingId = payment.BookingId.Value,
                Status = "Confirmado"
            };

            await _eventBus.PublishMessageAsync(bookingUpdate, "booking_queue", "UpdateBookingStatus");

            // Registrar Log
            await _userLogService.LogAsync(new UserLogDto
            {
                UserId = payment.UserId.Value,
                Title = "Pago Confirmado",
                Description = $"El pago para la reserva {payment.BookingId.Value} ha sido capturado exitosamente."
            });

            // Notificar al usuario con la factura
            var notification = new PaymentNotificationDto
            {
                BookingId = payment.BookingId.Value,
                UserId = payment.UserId.Value,
                UserName = payment.UserName,
                UserEmail = payment.UserEmail,
                Total = payment.Total.Value,
                Currency = payment.Currency.Value,
                Status = "Confirmado",
                InvoicePdf = pdfBytes
            };

            await _notificationBus.PublishMessageAsync(notification, "notifications_queue", "PaymentConfirmed");

            return true;
        }
    }
}
