using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }
    }
}