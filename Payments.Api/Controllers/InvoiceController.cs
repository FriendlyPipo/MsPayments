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
                var pdfBytes = await _mediator.Send(new GetInvoicePdfQuery { InvoiceId = id });
                return File(pdfBytes, "application/pdf", $"factura_{id}.pdf");
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}
