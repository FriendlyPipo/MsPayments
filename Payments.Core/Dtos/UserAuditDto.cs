using System;

namespace Payments.Core.Dtos
{
    public record UserAuditDto
    {
        public Guid? UserId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string JsonData { get; init; } = string.Empty;
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
