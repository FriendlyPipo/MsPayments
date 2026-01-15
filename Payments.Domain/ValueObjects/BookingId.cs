using System;

namespace Payments.Domain.ValueObjects
{
    public record BookingId(Guid Value)
    {
        public static BookingId Create(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("El ID de Reserva no puede estar vacio", nameof(value));
            }
            return new BookingId(value);
        }
    }
}
