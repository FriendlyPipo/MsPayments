using MediatR;
using Payments.Application.Queries;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using Payments.Domain.ValueObjects;
using System;

namespace Payments.Application.Handlers.Queries
{
    public class GetInvoicePdfQueryHandler : IRequestHandler<GetInvoicePdfQuery, byte[]>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPdfService<Invoice> _pdfService;

        public GetInvoicePdfQueryHandler(IInvoiceRepository invoiceRepository, IPdfService<Invoice> pdfService)
        {
            _invoiceRepository = invoiceRepository;
            _pdfService = pdfService;
        }

        public async Task<byte[]> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(InvoiceId.Create(request.InvoiceId));
            
            if (invoice == null)
            {
                throw new Exception("Factura no encontrada");
            }

            return await _pdfService.GeneratePdfAsync(invoice);
        }
    }
}
