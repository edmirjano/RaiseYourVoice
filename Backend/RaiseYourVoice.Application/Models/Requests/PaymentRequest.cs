using RaiseYourVoice.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using RaiseYourVoice.Application.Models;

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
        public required string CampaignId { get; set; }

        /// <summary>
        /// Amount to donate
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code (e.g., "USD", "EUR")
        /// </summary>
        [Required]
        public required string Currency { get; set; }

        /// <summary>
        /// Description or message with the donation
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Payment method details
        /// </summary>
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Customer information for the payment
        /// </summary>
        public CustomerInformation? CustomerInfo { get; set; }
    }

    /// <summary>
    /// Represents customer information for payment processing
    /// </summary>
    public class CustomerInformation
    {
        /// <summary>
        /// Full name of the customer
        /// </summary>
        [Required]
        public required string FullName { get; set; }

        /// <summary>
        /// Email address of the customer
        /// </summary>
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        /// <summary>
        /// Phone number of the customer
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Customer's billing address
        /// </summary>
        public string? BillingAddress { get; set; }

        /// <summary>
        /// Customer's city
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Customer's country
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Customer's postal/zip code
        /// </summary>
        public string? PostalCode { get; set; }
    }
}