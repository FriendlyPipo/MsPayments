using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payments.Application.Commands;
using Stripe;
using System.IO;
using System.Threading.Tasks;

namespace Payments.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly string _webhookSecret;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(IMediator mediator, IConfiguration configuration, ILogger<StripeWebhookController> logger)
        {
            _mediator = mediator;
            _webhookSecret = configuration["Stripe:WebhookSecret"] ?? "";
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                _logger.LogInformation($"Evento de Stripe recibido: {stripeEvent.Type}");

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        await _mediator.Send(new CapturePaymentCommand { StripeId = paymentIntent.Id });
                        _logger.LogInformation($"Pago confirmado vía webhook para StripeId: {paymentIntent.Id}");
                    }
                }
                else if (stripeEvent.Type == "payment_intent.payment_failed")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        var errorMessage = paymentIntent.LastPaymentError?.Message ?? "Pago fallido en Stripe";
                        await _mediator.Send(new FailPaymentCommand { StripeId = paymentIntent.Id, ErrorMessage = errorMessage });
                        _logger.LogWarning($"Pago fallido vía webhook para StripeId: {paymentIntent.Id}. Error: {errorMessage}");
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError($"Error de Stripe Webhook: {e.Message}");
                return BadRequest();
            }
        }
    }
}
