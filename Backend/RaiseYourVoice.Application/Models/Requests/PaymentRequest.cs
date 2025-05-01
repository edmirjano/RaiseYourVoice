using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for processing a payment/donation
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>
        /// ID of the campaign being donated to
        /// </summary>
        [Required]
        public string CampaignId { get; set; }
        
        /// <summary>
        /// Amount to donate
        /// </summary>
        [Required]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Currency code (e.g., "USD", "EUR")
        /// </summary>
        [Required]
        public string Currency { get; set; }
        
        /// <summary>
        /// Description or message with the donation
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Payment method details
        /// </summary>
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        /// <summary>
        /// Customer information for the payment
        /// </summary>
        public CustomerInformation CustomerInfo { get; set; }
    }
}