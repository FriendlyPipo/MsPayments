using System;

namespace Payments.Domain.ValueObjects
{
    public record UserId(Guid Value)
    {
        public static UserId Create(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("El ID de Usuario no puede estar vacio", nameof(value));
            }
            return new UserId(value);
        }
    }
}
