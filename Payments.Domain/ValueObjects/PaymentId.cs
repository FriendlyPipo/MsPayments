using System;

namespace Payments.Domain.ValueObjects
{
    public record PaymentId(Guid Value)
    {
        public static PaymentId Create(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("El ID de Pago no puede estar vacio", nameof(value));
            }
            return new PaymentId(value);
        }
    }
}
