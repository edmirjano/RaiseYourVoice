using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for user login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}