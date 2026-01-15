using System;

namespace Payments.Core.Exceptions
{
    public class RabbitMQException : Exception
    {
        public RabbitMQException(string message) : base(message)
        {
        }

        public RabbitMQException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
