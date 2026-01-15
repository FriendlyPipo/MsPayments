using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Commands;
using Payments.Application.Queries;
using System;
using System.Threading.Tasks;

namespace Payments.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentCommand command)
        {
            var clientSecret = await _mediator.Send(command);
            return Ok(new { clientSecret });
        }

        [HttpPost("capture")]
        public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound(new { message = "Pago no encontrado o no se pudo capturar" });
            return Ok(new { message = "Pago capturado exitosamente" });
        }

        [HttpPost("fail")]
        public async Task<IActionResult> FailPayment([FromBody] FailPaymentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound(new { message = "Pago no encontrado" });
            return Ok(new { message = "Pago marcado como fallido" });
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBookingId(Guid bookingId)
        {
            var query = new GetPaymentByBookingIdQuery(bookingId);
            var result = await _mediator.Send(query);
            
            if (result == null) return NotFound(new { message = "Pago no encontrado para esta reserva" });
            
            return Ok(result);
        }
    }
}
