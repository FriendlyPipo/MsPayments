using Microsoft.Extensions.Configuration;
using Payments.Core.Dtos;
using Payments.Core.Services;
using Stripe;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Payments.Infrastructure.Services
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly Func<PaymentIntentService> _serviceFactory;

        public StripeService(IConfiguration configuration) : this(configuration, () => new PaymentIntentService())
        {
        }

        public StripeService(IConfiguration configuration, Func<PaymentIntentService> serviceFactory)
        {
            _configuration = configuration;
            _serviceFactory = serviceFactory;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<StripePaymentIntentDto> CreatePaymentIntentAsync(decimal amount, string currency, string bookingId)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Se multiplica por 100 porque Stripe usa centavos
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "BookingId", bookingId }
                }
            };

            var service = _serviceFactory();
            var paymentIntent = await service.CreateAsync(options);

            return new StripePaymentIntentDto
            {
                StripeId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = (decimal)paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency,
                Status = paymentIntent.Status
            };
        }

        public async Task<bool> CancelPaymentIntentAsync(string stripeId)
        {
            var service = _serviceFactory();
            var paymentIntent = await service.CancelAsync(stripeId);
            return paymentIntent.Status == "canceled";
        }

        public async Task<StripePaymentIntentDto?> GetPaymentIntentAsync(string stripeId)
        {
            try
            {
                var service = _serviceFactory();
                var paymentIntent = await service.GetAsync(stripeId);
                return new StripePaymentIntentDto
                {
                    StripeId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Amount = (decimal)paymentIntent.Amount / 100m,
                    Currency = paymentIntent.Currency,
                    Status = paymentIntent.Status
                };
            }
            catch (StripeException)
            {
                return null;
            }
        }
    }
}