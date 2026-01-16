using System;

namespace Payments.Domain.ValueObjects
{
    public record InvoiceId(Guid Value)
    {
        public static InvoiceId Create(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("El ID de la Factura no puede estar vacío", nameof(value));
            }
            return new InvoiceId(value);
        }
    }
}
