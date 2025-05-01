using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using RaiseYourVoice.Application.Models.Requests;

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

    public class PaymentMethodInfo
    {
        public required string Type { get; set; } // "card", "bank_transfer", etc.
        public required string CardNumber { get; set; }
        public required string ExpiryMonth { get; set; }
        public required string ExpiryYear { get; set; }
        public required string Cvc { get; set; }
        public required string CardholderName { get; set; }
        public string? TokenId { get; set; } // For tokenized payment methods
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? CustomerId { get; set; }
        public string? PaymentMethodId { get; set; }
    }
}