using MediatR;
using System;
using Payments.Core.Dtos;

namespace Payments.Application.Queries
{
    public class GetPaymentByBookingIdQuery : IRequest<GetPaymentDto?>
    {
        public Guid BookingId { get; set; }

        public GetPaymentByBookingIdQuery(Guid bookingId)
        {
            BookingId = bookingId;
        }
    }
}
