using System;
using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public string SentBy { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public TargetAudience TargetAudience { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }
        public ReadStatus ReadStatus { get; set; }
    }

    public class TargetAudience
    {
        public TargetType Type { get; set; }
        public string[] UserIds { get; set; }
        public UserRole[] TargetRoles { get; set; }
        public string[] Topics { get; set; }
        public string[] Regions { get; set; }
    }
}