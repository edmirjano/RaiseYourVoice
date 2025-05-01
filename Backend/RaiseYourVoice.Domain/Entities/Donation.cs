using System;
using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Domain.Entities
{
    public class Donation : BaseEntity
    {
        public string CampaignId { get; set; }
        public Campaign Campaign { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public decimal Amount { get; set; }
        public bool IsAnonymous { get; set; }
        public string Message { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        
        [Encrypted]
        public string TransactionId { get; set; }
        
        [Encrypted]
        public string PaymentMethod { get; set; }
        
        public string Currency { get; set; } = "USD";
        
        [Encrypted]
        public string ReceiptUrl { get; set; }
        
        public DateTime? RefundedAt { get; set; }
        public string RefundReason { get; set; }
        public bool IsSubscriptionDonation { get; set; }
        
        [Encrypted]
        public string SubscriptionId { get; set; }
        
        public DonorInformation DonorInformation { get; set; }
    }

    public class DonorInformation
    {
        [Encrypted]
        public string Email { get; set; }
        
        [Encrypted]
        public string FullName { get; set; }
        
        [Encrypted]
        public string Address { get; set; }
        
        [Encrypted]
        public string City { get; set; }
        
        [Encrypted]
        public string State { get; set; }
        
        public string Country { get; set; }
        
        [Encrypted]
        public string PostalCode { get; set; }
        
        [Encrypted]
        public string Phone { get; set; }
        
        public bool IsTaxReceiptRequested { get; set; }
    }
}