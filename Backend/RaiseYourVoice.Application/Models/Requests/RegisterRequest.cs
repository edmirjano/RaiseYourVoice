using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for user registration
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// User's full name
        /// </summary>
        [Required]
        public string Name { get; set; }
        
        /// <summary>
        /// User's email address (used for login)
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        
        /// <summary>
        /// URL to user's profile picture
        /// </summary>
        public string ProfilePicture { get; set; }
        
        /// <summary>
        /// Short biography or description
        /// </summary>
        public string Bio { get; set; }
        
        /// <summary>
        /// User's preferred language code (e.g., "en", "sq")
        /// </summary>
        public string PreferredLanguage { get; set; }
    }
}