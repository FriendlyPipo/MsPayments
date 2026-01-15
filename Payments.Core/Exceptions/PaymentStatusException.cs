using System;

namespace Payments.Core.Exceptions
{
    public class PaymentStatusException : Exception
    {
        public PaymentStatusException(string message) : base(message)
        {
        }
    }
}
