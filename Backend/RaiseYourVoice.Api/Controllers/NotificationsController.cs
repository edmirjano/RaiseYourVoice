using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IPushNotificationService _pushNotificationService;

        public NotificationsController(
            IGenericRepository<Notification> notificationRepository,
            IGenericRepository<User> userRepository,
            IPushNotificationService pushNotificationService)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _pushNotificationService = pushNotificationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUserNotifications()
        {
            var userId = User.Identity.Name;
            var allNotifications = await _notificationRepository.GetAllAsync();
            
            // Filter notifications relevant to the current user
            var userNotifications = allNotifications.Where(n => 
                (n.TargetAudience.Type == TargetType.AllUsers) ||
                (n.TargetAudience.Type == TargetType.SpecificUsers && 
                 n.TargetAudience.UserIds != null && 
                 n.TargetAudience.UserIds.Contains(userId)) ||
                (n.TargetAudience.Type == TargetType.ByRole && 
                 User.IsInRole(string.Join(",", n.TargetAudience.TargetRoles?.Select(r => r.ToString()) ?? Array.Empty<string>())))
            ).OrderByDescending(n => n.SentAt);
            
            return Ok(userNotifications);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Notification>> GetNotification(string id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<ActionResult<Notification>> CreateNotification(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.SentAt = DateTime.UtcNow;
            notification.SentBy = User.Identity.Name;
            notification.DeliveryStatus = DeliveryStatus.Sent;
            notification.ReadStatus = ReadStatus.Unread;
            
            await _notificationRepository.AddAsync(notification);
            
            // Send push notifications based on target audience
            await SendPushNotificationsAsync(notification);
            
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpPut("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            
            notification.ReadStatus = ReadStatus.Read;
            notification.UpdatedAt = DateTime.UtcNow;
            
            var success = await _notificationRepository.UpdateAsync(notification);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpPut("{id}/dismiss")]
        [Authorize]
        public async Task<IActionResult> DismissNotification(string id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            
            notification.ReadStatus = ReadStatus.Dismissed;
            notification.UpdatedAt = DateTime.UtcNow;
            
            var success = await _notificationRepository.UpdateAsync(notification);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            var success = await _notificationRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpPost("broadcast")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<ActionResult<Notification>> BroadcastNotification(Notification notification)
        {
            // Set up a broadcast notification
            notification.CreatedAt = DateTime.UtcNow;
            notification.SentAt = DateTime.UtcNow;
            notification.SentBy = User.Identity.Name;
            notification.DeliveryStatus = DeliveryStatus.Sent;
            notification.ReadStatus = ReadStatus.Unread;
            
            if (notification.TargetAudience == null)
            {
                notification.TargetAudience = new TargetAudience
                {
                    Type = TargetType.AllUsers
                };
            }
            
            await _notificationRepository.AddAsync(notification);
            
            // Send to all users via push notification
            int sentCount = await _pushNotificationService.BroadcastAsync(notification);
            
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }
        
        [HttpPost("device-token")]
        [Authorize]
        public async Task<IActionResult> RegisterDeviceToken(DeviceTokenRequest request)
        {
            string userId = User.Identity.Name;
            await _pushNotificationService.RegisterDeviceTokenAsync(userId, request.DeviceToken, request.DeviceType);
            return Ok();
        }
        
        [HttpDelete("device-token/{token}")]
        [Authorize]
        public async Task<IActionResult> RemoveDeviceToken(string token)
        {
            string userId = User.Identity.Name;
            await _pushNotificationService.RemoveDeviceTokenAsync(userId, token);
            return Ok();
        }
        
        private async Task SendPushNotificationsAsync(Notification notification)
        {
            switch (notification.TargetAudience.Type)
            {
                case TargetType.AllUsers:
                    await _pushNotificationService.BroadcastAsync(notification);
                    break;
                    
                case TargetType.SpecificUsers:
                    if (notification.TargetAudience.UserIds != null && notification.TargetAudience.UserIds.Length > 0)
                    {
                        await _pushNotificationService.SendToUsersAsync(notification.TargetAudience.UserIds, notification);
                    }
                    break;
                    
                case TargetType.ByRole:
                    if (notification.TargetAudience.TargetRoles != null && notification.TargetAudience.TargetRoles.Length > 0)
                    {
                        foreach (var role in notification.TargetAudience.TargetRoles)
                        {
                            await _pushNotificationService.SendToRoleAsync(role.ToString(), notification);
                        }
                    }
                    break;
                    
                // Additional targeting options could be implemented here
            }
        }
    }
    
    public class DeviceTokenRequest
    {
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; } // e.g., "ios", "android"
    }
}