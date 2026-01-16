using System;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Entities
{
    public class Invoice
    {
        public InvoiceId InvoiceId { get; private set; }
        public PaymentId PaymentId { get; private set; }
        public UserId UserId { get; private set; }
        public InvoiceTotal Total { get; private set; }
        public InvoiceCurrency Currency { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string UserName { get; private set; }
        public string UserEmail { get; private set; }

        private Invoice(InvoiceId invoiceId, PaymentId paymentId, UserId userId, InvoiceTotal total, InvoiceCurrency currency, DateTime createdAt, string userName, string userEmail)
        {
            InvoiceId = invoiceId;
            PaymentId = paymentId;
            UserId = userId;
            UserName = userName;
            Total = total;
            Currency = currency;
            CreatedAt = createdAt;  
            UserEmail = userEmail;
        }

        public static Invoice Create(InvoiceId invoiceId, PaymentId paymentId, UserId userId, InvoiceTotal total, InvoiceCurrency currency, string userName, string userEmail)
        {
            return new Invoice(invoiceId, paymentId, userId, total, currency, DateTime.UtcNow, userName, userEmail);
        }
    }   
}
