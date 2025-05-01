using System;
using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string ReplacedByToken { get; set; }
        public string ReasonRevoked { get; set; }
    }
}