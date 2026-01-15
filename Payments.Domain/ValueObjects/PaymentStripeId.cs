using System;

namespace Payments.Domain.ValueObjects
{
    public record PaymentStripeId(string Value)
    {
        public static PaymentStripeId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El ID de Stripe no puede estar vacio", nameof(value));
            }
            return new PaymentStripeId(value);
        }
    }
}
