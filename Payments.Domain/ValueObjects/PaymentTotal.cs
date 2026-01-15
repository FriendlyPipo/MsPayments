using System;

namespace Payments.Domain.ValueObjects
{
    public record PaymentTotal(decimal Value)
    {
        public static PaymentTotal Create(decimal value)
        {
            if (value < 0)
            {
                throw new ArgumentException("El total del pago no puede ser negativo", nameof(value));
            }
            return new PaymentTotal(value);
        }
    }
}
