using MediatR;
using System;

namespace Payments.Application.Queries
{
    public class GetInvoicePdfQuery : IRequest<byte[]>
    {
        public Guid InvoiceId { get; set; }
    }
}
