using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly IConfiguration _configuration;

        public PushNotificationService(
            IGenericRepository<User> userRepository,
            ILogger<PushNotificationService> logger,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendToUserAsync(string userId, Notification notification)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || !user.DeviceTokens.Any() || !user.NotificationSettings.PushNotifications)
                {
                    return false;
                }

                await SendPushNotificationsToDevices(user.DeviceTokens, notification);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
                return false;
            }
        }

        public async Task<int> SendToUsersAsync(IEnumerable<string> userIds, Notification notification)
        {
            int successCount = 0;
            foreach (var userId in userIds)
            {
                if (await SendToUserAsync(userId, notification))
                {
                    successCount++;
                }
            }
            return successCount;
        }

        public async Task<int> SendToRoleAsync(string role, Notification notification)
        {
            // Parse string role to enum
            if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                _logger.LogWarning("Invalid role provided: {Role}", role);
                return 0;
            }

            var users = await _userRepository.FindAsync(u => u.Role == userRole);
            return await SendToUsersAsync(users.Select(u => u.Id), notification);
        }

        public async Task<int> BroadcastAsync(Notification notification)
        {
            var users = await _userRepository.GetAllAsync();
            return await SendToUsersAsync(users.Select(u => u.Id), notification);
        }

        public async Task RegisterDeviceTokenAsync(string userId, string deviceToken, string deviceType)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempted to register device token for non-existent user {UserId}", userId);
                    return;
                }

                // If token doesn't already exist, add it
                if (!user.DeviceTokens.Contains(deviceToken))
                {
                    user.DeviceTokens.Add(deviceToken);
                    await _userRepository.UpdateAsync(user);
                    _logger.LogInformation("Device token registered for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token for user {UserId}", userId);
            }
        }

        public async Task RemoveDeviceTokenAsync(string userId, string deviceToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempted to remove device token for non-existent user {UserId}", userId);
                    return;
                }

                if (user.DeviceTokens.Contains(deviceToken))
                {
                    user.DeviceTokens.Remove(deviceToken);
                    await _userRepository.UpdateAsync(user);
                    _logger.LogInformation("Device token removed for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing device token for user {UserId}", userId);
            }
        }

        private async Task SendPushNotificationsToDevices(List<string> deviceTokens, Notification notification)
        {
            // This method would implement the actual push notification delivery
            // For production, you would integrate with Firebase Cloud Messaging for Android
            // and Apple Push Notification service for iOS
            
            // For now, we'll just log that we would send the notification
            _logger.LogInformation("Would send push notification \"{Title}\" to {Count} devices", 
                notification.Title, deviceTokens.Count);
            
            // In a real implementation, you would:
            // 1. Format the notification payload for each platform
            // 2. Send the notification to the respective service (FCM/APNs)
            // 3. Handle response/errors from the service
            // 4. Update delivery status
            
            await Task.CompletedTask; // Placeholder for async operation
        }
    }
}