using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Name { get; set; }
        
        [Encrypted]
        public required string Email { get; set; }
        
        [Encrypted]
        public required string PasswordHash { get; set; }
        
        public UserRole Role { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastLogin { get; set; }
        public required string PreferredLanguage { get; set; } = "en";
        public NotificationSettings NotificationSettings { get; set; } = new NotificationSettings();
        public List<ExternalAuthProvider> ExternalAuthProviders { get; set; } = new List<ExternalAuthProvider>();
        
        [Encrypted]
        public List<string> DeviceTokens { get; set; } = new List<string>();
    }

    public class NotificationSettings
    {
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool NewPostNotifications { get; set; } = true;
        public bool CommentNotifications { get; set; } = true;
        public bool EventReminders { get; set; } = true;
        public string PreferredNotificationTime { get; set; } = "09:00";
    }

    public class ExternalAuthProvider
    {
        public ExternalAuthType Type { get; set; }
        public required string ExternalId { get; set; }
        public required string ExternalUsername { get; set; }
        public DateTime ConnectedAt { get; set; }
        
        [Encrypted]
        public required string AccessToken { get; set; }
        
        public DateTime? TokenExpiry { get; set; }
    }
}