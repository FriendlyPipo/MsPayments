using MediatR;
using Payments.Core.Dtos;
using System;

namespace Payments.Application.Queries
{
    public record GetInvoiceByPaymentIdQuery(Guid PaymentId) : IRequest<GetInvoiceDto>;
}
