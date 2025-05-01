using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for rejecting a campaign with a reason
    /// </summary>
    public class RejectionReason
    {
        /// <summary>
        /// The reason for rejecting the campaign
        /// </summary>
        [Required]
        public string Reason { get; set; }
    }
}