using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Application.Interfaces
{
    public interface IPaymentGateway
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, string reason);
        Task<string> CreateCustomerAsync(DonorInformation customerInfo);
        Task<string> SavePaymentMethodAsync(string customerId, PaymentMethodInfo paymentMethod);
        Task<string> CreateSubscriptionAsync(string customerId, string paymentMethodId, decimal amount, string description);
        Task<bool> CancelSubscriptionAsync(string subscriptionId);
        Task<PaymentStatus> GetPaymentStatusAsync(string transactionId);
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public required string Description { get; set; }
        public required DonorInformation CustomerInfo { get; set; }
        public required PaymentMethodInfo PaymentMethod { get; set; }
        public required string CampaignId { get; set; }
        public bool SavePaymentMethod { get; set; }
    }

    public class PaymentMethodInfo
    {
        public required string Type { get; set; } // "card", "bank_transfer", etc.
        public required string CardNumber { get; set; }
        public required string ExpiryMonth { get; set; }
        public required string ExpiryYear { get; set; }
        public required string Cvc { get; set; }
        public required string CardholderName { get; set; }
        public required string TokenId { get; set; } // For tokenized payment methods
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public required string TransactionId { get; set; }
        public required string ErrorMessage { get; set; }
        public PaymentStatus Status { get; set; }
        public required string ReceiptUrl { get; set; }
        public required string CustomerId { get; set; }
        public required string PaymentMethodId { get; set; }
    }
}