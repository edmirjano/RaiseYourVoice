using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Application.Interfaces
{
    public interface IDonationService
    {
        Task<Donation> CreateDonationAsync(Donation donation);
        Task<IEnumerable<Donation>> GetDonationsByCampaignAsync(string campaignId);
        Task<IEnumerable<Donation>> GetDonationsByUserAsync(string userId);
        Task<IEnumerable<Donation>> GetDonationsByTransactionIdAsync(string transactionId);
        Task<Donation> GetDonationByIdAsync(string id);
        Task<bool> UpdateDonationStatusAsync(string id, PaymentStatus status);
        Task<bool> RefundDonationAsync(string id, string reason);
        Task<Dictionary<string, decimal>> GetDonationStatisticsAsync(string campaignId);
        Task<Dictionary<string, object>> GetDonationInsightsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<string> GenerateDonationReceiptAsync(string donationId);
        Task<bool> CreateSubscriptionDonationAsync(string userId, string campaignId, decimal amount, string paymentMethodId);
        Task<bool> CancelSubscriptionDonationAsync(string subscriptionId);
    }
}