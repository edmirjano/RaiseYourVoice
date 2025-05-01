using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Application.Interfaces
{
    public interface IPushNotificationService
    {
        /// <summary>
        /// Send a push notification to a specific user
        /// </summary>
        Task<bool> SendToUserAsync(string userId, Notification notification);
        
        /// <summary>
        /// Send a push notification to multiple users
        /// </summary>
        Task<int> SendToUsersAsync(IEnumerable<string> userIds, Notification notification);
        
        /// <summary>
        /// Send a push notification to all users with a specific role
        /// </summary>
        Task<int> SendToRoleAsync(string role, Notification notification);
        
        /// <summary>
        /// Send a push notification to all users
        /// </summary>
        Task<int> BroadcastAsync(Notification notification);
        
        /// <summary>
        /// Register or update a device token for a user
        /// </summary>
        Task RegisterDeviceTokenAsync(string userId, string deviceToken, string deviceType);
        
        /// <summary>
        /// Remove a device token for a user (e.g., on logout)
        /// </summary>
        Task RemoveDeviceTokenAsync(string userId, string deviceToken);

        /// <summary>
        /// Send a simple notification to a user with just title and content
        /// </summary>
        Task<bool> SendNotificationAsync(string userId, string title, string content);

        /// <summary>
        /// Send a notification to admin users
        /// </summary>
        Task<bool> SendAdminNotificationAsync(string title, string content);

        /// <summary>
        /// Send a notification to all users who donated to a campaign
        /// </summary>
        Task<bool> SendCampaignUpdateNotificationAsync(string campaignId, string title, string content);
    }
}