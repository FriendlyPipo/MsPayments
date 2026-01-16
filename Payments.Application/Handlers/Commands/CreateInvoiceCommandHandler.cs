using MediatR;
using Payments.Application.Commands;
using Payments.Core.Repositories;
using Payments.Domain.Entities;
using Payments.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payments.Application.Handlers.Commands
{
    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, bool>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public CreateInvoiceCommandHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<bool> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = Invoice.Create(
                InvoiceId.Create(Guid.NewGuid()),
                PaymentId.Create(request.PaymentId),
                UserId.Create(request.UserId),
                InvoiceTotal.Create(request.Total),
                InvoiceCurrency.Create(request.Currency),
                request.UserName,
                request.UserEmail
            );

            await _invoiceRepository.CreateAsync(invoice);

            return true;
        }
    }
}
