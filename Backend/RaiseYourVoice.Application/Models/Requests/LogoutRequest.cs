namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for user logout
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// The refresh token to revoke during logout
        /// </summary>
        public string RefreshToken { get; set; }
    }
}