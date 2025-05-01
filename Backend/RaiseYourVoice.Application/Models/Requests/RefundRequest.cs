using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for refunding a donation
    /// </summary>
    public class RefundRequest
    {
        /// <summary>
        /// Reason for the refund
        /// </summary>
        [Required]
        public string Reason { get; set; }
    }
}