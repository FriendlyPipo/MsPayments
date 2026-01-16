using System;

namespace Payments.Domain.ValueObjects
{
    public record InvoiceTotal(decimal Value)
    {
        public static InvoiceTotal Create(decimal value)
        {
            if (value < 0)
            {
                throw new ArgumentException("El total de la factura no puede ser negativo", nameof(value));
            }
            return new InvoiceTotal(value);
        }
    }
}
