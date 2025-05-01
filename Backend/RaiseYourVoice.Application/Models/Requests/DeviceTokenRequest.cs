using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for registering a device token for push notifications
    /// </summary>
    public class DeviceTokenRequest
    {
        /// <summary>
        /// The device token for push notifications
        /// </summary>
        [Required]
        public string DeviceToken { get; set; }
        
        /// <summary>
        /// The type of device (e.g., "ios", "android")
        /// </summary>
        [Required]
        public string DeviceType { get; set; }
    }
}