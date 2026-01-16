using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Queries;
using System;
using System.Threading.Tasks;

namespace Payments.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvoiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadInvoice(Guid id)
        {
            try
            {
                var pdfBytes = await _mediator.Send(new GetInvoicePdfQuery(id));
                return File(pdfBytes, "application/pdf", $"factura_{id}.pdf");
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpGet("payment/{paymentId}")]
        public async Task<IActionResult> GetByPaymentId(Guid paymentId)
        {
            var result = await _mediator.Send(new GetInvoiceByPaymentIdQuery(paymentId));
            
            if (result == null)
            {
                return NotFound(new { Message = "Factura no encontrada para el pago especificado" });
            }

            return Ok(result);
        }
    }
}
