using System;

namespace Payments.Domain.ValueObjects
{
    public record InvoiceCurrency(string Value)
    {
        public static InvoiceCurrency Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("La moneda de la factura no puede estar vacía", nameof(value));
            }
            return new InvoiceCurrency(value);
        }
    }
}
