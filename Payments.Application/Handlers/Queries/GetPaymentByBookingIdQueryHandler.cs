using MediatR;
using Payments.Application.Queries;
using Payments.Core.Dtos;
using Payments.Core.Repositories;
using Payments.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace Payments.Application.Handlers.Queries
{
    public class GetPaymentByBookingIdQueryHandler : IRequestHandler<GetPaymentByBookingIdQuery, GetPaymentDto?>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByBookingIdQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<GetPaymentDto?> Handle(GetPaymentByBookingIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetPaymentByBookingIdAsync(
                BookingId.Create(request.BookingId), 
                cancellationToken);

            if (payment == null) return null;

            return new GetPaymentDto
            {
                PaymentId = payment.PaymentId.Value,
                BookingId = payment.BookingId.Value,
                UserId = payment.UserId.Value,
                StripeId = payment.StripeId.Value,
                Total = payment.Total.Value,
                Currency = payment.Currency.Value,
                Status = payment.Status.ToString(),
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}
