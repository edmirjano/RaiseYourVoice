using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public NotificationType Type { get; set; }
        public required string SentBy { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public required TargetAudience TargetAudience { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }
        public ReadStatus ReadStatus { get; set; }
    }

    public class TargetAudience
    {
        public TargetType Type { get; set; }
        public string[]? UserIds { get; set; }
        public UserRole[]? TargetRoles { get; set; }
        public string[]? Topics { get; set; }
        public string[]? Regions { get; set; }
    }
}