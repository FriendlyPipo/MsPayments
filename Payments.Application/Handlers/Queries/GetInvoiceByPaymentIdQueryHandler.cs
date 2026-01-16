using MediatR;
using Payments.Application.Queries;
using Payments.Core.Dtos;
using Payments.Core.Repositories;
using Payments.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Payments.Application.Handlers.Queries
{
    public class GetInvoiceByPaymentIdQueryHandler : IRequestHandler<GetInvoiceByPaymentIdQuery, GetInvoiceDto?>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public GetInvoiceByPaymentIdQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<GetInvoiceDto?> Handle(GetInvoiceByPaymentIdQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByPaymentIdAsync(PaymentId.Create(request.PaymentId));

            if (invoice == null)
            {
                return null;
            }

            return new GetInvoiceDto
            {
                InvoiceId = invoice.InvoiceId.Value,
                PaymentId = invoice.PaymentId.Value,
                UserId = invoice.UserId.Value,
                UserName = invoice.UserName,
                UserEmail = invoice.UserEmail,
                Total = invoice.Total.Value,
                Currency = invoice.Currency.Value,
                CreatedAt = invoice.CreatedAt
            };
        }
    }
}
