using System.Threading.Tasks;
using Payments.Core.Dtos;

namespace Payments.Core.Services
{
    public interface IStripeService
    {
        Task<StripePaymentIntentDto> CreatePaymentIntentAsync(decimal amount, string currency, string bookingId);
        Task<bool> CancelPaymentIntentAsync(string stripeId);
        Task<StripePaymentIntentDto?> GetPaymentIntentAsync(string stripeId);
    }
}
