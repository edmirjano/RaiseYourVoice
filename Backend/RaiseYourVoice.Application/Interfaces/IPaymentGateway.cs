using System.Threading.Tasks;
using RaiseYourVoice.Domain.Entities;

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
        public string Description { get; set; }
        public DonorInformation CustomerInfo { get; set; }
        public PaymentMethodInfo PaymentMethod { get; set; }
        public string CampaignId { get; set; }
        public bool SavePaymentMethod { get; set; }
    }

    public class PaymentMethodInfo
    {
        public string Type { get; set; } // "card", "bank_transfer", etc.
        public string CardNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string Cvc { get; set; }
        public string CardholderName { get; set; }
        public string TokenId { get; set; } // For tokenized payment methods
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public PaymentStatus Status { get; set; }
        public string ReceiptUrl { get; set; }
        public string CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
    }
}