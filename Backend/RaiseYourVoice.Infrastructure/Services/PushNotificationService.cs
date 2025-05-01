using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
                if (user == null || user.DeviceTokens == null || user.DeviceTokens.Count == 0)
                {
                    _logger.LogWarning("No device tokens found for user {UserId}", userId);
                    return false;
                }

                // Check if user has enabled push notifications
                if (!user.Preferences.NotificationSettings.PushNotifications)
                {
                    _logger.LogInformation("User {UserId} has disabled push notifications", userId);
                    return false;
                }

                // Send notification to all user devices
                foreach (var deviceToken in user.DeviceTokens)
                {
                    await SendPushNotificationAsync(deviceToken, notification.Title, notification.Content);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
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
            try
            {
                // Parse the role string to UserRole enum
                if (!Enum.TryParse(role, true, out UserRole userRole))
                {
                    _logger.LogError("Invalid role: {Role}", role);
                    return 0;
                }

                // Get all users with the specified role
                var allUsers = await _userRepository.GetAllAsync();
                var usersWithRole = allUsers.Where(u => u.Role == userRole).ToList();

                if (!usersWithRole.Any())
                {
                    _logger.LogWarning("No users found with role {Role}", role);
                    return 0;
                }

                // Send notification to all users with the role
                int successCount = 0;
                foreach (var user in usersWithRole)
                {
                    if (await SendToUserAsync(user.Id, notification))
                    {
                        successCount++;
                    }
                }

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to role {Role}", role);
                return 0;
            }
        }

        public async Task<int> BroadcastAsync(Notification notification)
        {
            try
            {
                // Get all users
                var users = await _userRepository.GetAllAsync();
                
                // Send notification to all users
                int successCount = 0;
                foreach (var user in users)
                {
                    if (await SendToUserAsync(user.Id, notification))
                    {
                        successCount++;
                    }
                }

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast notification");
                return 0;
            }
        }

        public async Task RegisterDeviceTokenAsync(string userId, string deviceToken, string deviceType)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogError("User not found: {UserId}", userId);
                    return;
                }

                // Initialize DeviceTokens if null
                if (user.DeviceTokens == null)
                {
                    user.DeviceTokens = new List<string>();
                }

                // Add token if it doesn't exist already
                if (!user.DeviceTokens.Contains(deviceToken))
                {
                    user.DeviceTokens.Add(deviceToken);
                    await _userRepository.UpdateAsync(user);
                    _logger.LogInformation("Device token registered for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register device token for user {UserId}", userId);
            }
        }

        public async Task RemoveDeviceTokenAsync(string userId, string deviceToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || user.DeviceTokens == null)
                {
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
                _logger.LogError(ex, "Failed to remove device token for user {UserId}", userId);
            }
        }

        // Implementation for methods used in WebhooksController and CampaignService
        public async Task<bool> SendNotificationAsync(string userId, string title, string content)
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Content = content,
                    Type = NotificationType.SystemAnnouncement,
                    SentBy = "system",
                    SentAt = DateTime.UtcNow,
                    TargetAudience = new TargetAudience
                    {
                        Type = TargetType.SpecificUsers,
                        UserIds = new[] { userId }
                    },
                    DeliveryStatus = DeliveryStatus.Queued,
                    ReadStatus = ReadStatus.Unread
                };

                return await SendToUserAsync(userId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> SendAdminNotificationAsync(string title, string content)
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Content = content,
                    Type = NotificationType.SystemAnnouncement,
                    SentBy = "system",
                    SentAt = DateTime.UtcNow,
                    TargetAudience = new TargetAudience
                    {
                        Type = TargetType.ByRole,
                        TargetRoles = new[] { UserRole.Admin, UserRole.Moderator }
                    },
                    DeliveryStatus = DeliveryStatus.Queued,
                    ReadStatus = ReadStatus.Unread
                };

                // Send to all admin and moderator users
                int adminCount = await SendToRoleAsync(UserRole.Admin.ToString(), notification);
                int modCount = await SendToRoleAsync(UserRole.Moderator.ToString(), notification);
                
                return adminCount + modCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin notification");
                return false;
            }
        }

        public async Task<bool> SendCampaignUpdateNotificationAsync(string campaignId, string title, string content)
        {
            try
            {
                // In a real implementation, we would fetch all donors for this campaign
                // and send them notifications. For simplicity, we're just logging here.
                _logger.LogInformation("Campaign update notification for {CampaignId}: {Title}", campaignId, title);
                
                // This would be replaced with actual campaign donor notification logic
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send campaign update notification");
                return false;
            }
        }

        private async Task<bool> SendPushNotificationAsync(string deviceToken, string title, string message)
        {
            // This method would integrate with a push notification provider
            // such as Firebase Cloud Messaging, Apple Push Notification Service, etc.
            
            // For development purposes, we'll just log the notification
            _logger.LogInformation(
                "Push notification sent to device {DeviceToken}: {Title} - {Message}",
                deviceToken, title, message);
            
            // Simulate async operation and success
            await Task.Delay(10);
            return true;
        }
    }
}