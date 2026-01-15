using System;

namespace Payments.Core.Exceptions
{
    public class PaymentNotFoundException : Exception
    {
        public PaymentNotFoundException(Guid paymentId) 
            : base($"El pago con ID {paymentId} no fue encontrado.")
        {
        }
    }
}
