using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}