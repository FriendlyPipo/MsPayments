using System;

namespace Payments.Core.Dtos
{
    public record UserLogDto
    {
        public Guid? UserId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}
