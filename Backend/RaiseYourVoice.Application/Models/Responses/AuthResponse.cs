using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Application.Models.Responses
{
    /// <summary>
    /// Response model for authentication operations
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Unique identifier of the authenticated user
        /// </summary>
        public required string UserId { get; set; }
        
        /// <summary>
        /// Name of the authenticated user
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Email address of the authenticated user
        /// </summary>
        public required string Email { get; set; }
        
        /// <summary>
        /// Role of the authenticated user
        /// </summary>
        public UserRole Role { get; set; }
        
        /// <summary>
        /// JWT access token for API authentication
        /// </summary>
        public required string Token { get; set; }
        
        /// <summary>
        /// Refresh token for obtaining new access tokens
        /// </summary>
        public required string RefreshToken { get; set; }
    }
}