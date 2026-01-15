using System;

namespace Payments.Domain.ValueObjects
{
    public record PaymentCurrency(string Value)
    {
        public static PaymentCurrency Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 3)
            {
                throw new ArgumentException("La moneda debe ser un codigo de 3 caracteres (ej: USD, PEN)", nameof(value));
            }
            return new PaymentCurrency(value.ToUpper());
        }
    }
}
