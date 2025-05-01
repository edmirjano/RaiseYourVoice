using RaiseYourVoice.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Application.Models.Requests
{
    public class PaymentRequest
    {
        [Required]
        public required string CampaignId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public required string Currency { get; set; } = "USD";

        public string? Description { get; set; }

        [Required]
        public required PaymentMethodInfo PaymentMethod { get; set; }

        public DonorInformation? CustomerInfo { get; set; }
        
        public bool SavePaymentMethod { get; set; }
    }

    public class CustomerInformation
    {
        [Required]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? BillingAddress { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? PostalCode { get; set; }
    }
}