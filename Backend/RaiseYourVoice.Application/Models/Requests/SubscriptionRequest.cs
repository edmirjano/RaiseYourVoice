using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for creating a recurring donation subscription
    /// </summary>
    public class SubscriptionRequest
    {
        /// <summary>
        /// ID of the campaign to donate to
        /// </summary>
        [Required]
        public required string CampaignId { get; set; }
        
        /// <summary>
        /// Amount to donate periodically
        /// </summary>
        [Required]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// ID of the saved payment method to use
        /// </summary>
        [Required]
        public required string PaymentMethodId { get; set; }
    }
}