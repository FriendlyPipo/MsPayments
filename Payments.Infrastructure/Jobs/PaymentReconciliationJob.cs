using MediatR;
using Microsoft.Extensions.Logging;
using Payments.Application.Commands;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Jobs
{
    public class PaymentReconciliationJob
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IStripeService _stripeService;
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentReconciliationJob> _logger;

        public PaymentReconciliationJob(
            IPaymentRepository paymentRepository,
            IStripeService stripeService,
            IMediator mediator,
            ILogger<PaymentReconciliationJob> logger)
        {
            _paymentRepository = paymentRepository;
            _stripeService = stripeService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task ReconcileAsync()
        {
            _logger.LogInformation("Iniciando Job de conciliacion de pagos...");

            var pendingPayments = await _paymentRepository.GetPendingPaymentsAsync();

            if (!pendingPayments.Any())
            {
                _logger.LogInformation("No hay pagos pendientes para conciliar.");
                return;
            }

            foreach (var payment in pendingPayments)
            {
                try
                {
                    _logger.LogInformation($"Conciliando pago {payment.PaymentId.Value} (StripeId: {payment.StripeId.Value})");

                    var stripeIntent = await _stripeService.GetPaymentIntentAsync(payment.StripeId.Value);

                    if (stripeIntent == null)
                    {
                        _logger.LogWarning($"No se encontro PaymentIntent en Stripe para el ID: {payment.StripeId.Value}");
                        continue;
                    }

                    if (stripeIntent.Status == "succeeded" && payment.Status != PaymentStatus.Exitoso)
                    {
                        _logger.LogInformation($"Pago {payment.PaymentId.Value} tuvo exito en Stripe.Actualizando estado");
                        await _mediator.Send(new CapturePaymentCommand { StripeId = payment.StripeId.Value });
                    }
                    else if ((stripeIntent.Status == "canceled" || stripeIntent.Status == "requires_payment_method") && payment.Status != PaymentStatus.Fallido)
                    {
                        _logger.LogWarning($"Pago {payment.PaymentId.Value} fallo o fue cancelado en Stripe (Status: {stripeIntent.Status}). Actualizando estado");
                        await _mediator.Send(new FailPaymentCommand { StripeId = payment.StripeId.Value, ErrorMessage = "Conciliado como fallido desde Stripe" });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al conciliar el pago {payment.PaymentId.Value}: {ex.Message}");
                }
            }

            _logger.LogInformation("Job de conciliacion de pagos finalizado.");
        }
    }
}
