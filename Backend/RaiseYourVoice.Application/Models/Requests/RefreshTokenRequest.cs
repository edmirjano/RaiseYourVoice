using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for refreshing an authentication token
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// The expired JWT token
        /// </summary>
        [Required]
        public required string Token { get; set; }
        
        /// <summary>
        /// The refresh token associated with the expired JWT
        /// </summary>
        [Required]
        public required string RefreshToken { get; set; }
    }
}