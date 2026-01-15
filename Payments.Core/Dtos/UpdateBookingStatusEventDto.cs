using System;

namespace Payments.Core.Dtos
{
    public class UpdateBookingStatusEventDto
    {
        public Guid BookingId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
