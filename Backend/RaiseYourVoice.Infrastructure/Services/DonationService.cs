using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Services
{
    public class DonationService : IDonationService
    {
        private readonly IMongoCollection<Donation> _donations;
        private readonly IMongoCollection<Campaign> _campaigns;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPushNotificationService _notificationService;
        private readonly ILogger<DonationService> _logger;

        public DonationService(
            IMongoClient mongoClient,
            IPaymentGateway paymentGateway,
            IPushNotificationService notificationService,
            ILogger<DonationService> logger)
        {
            var database = mongoClient.GetDatabase("RaiseYourVoice");
            _donations = database.GetCollection<Donation>("Donations");
            _campaigns = database.GetCollection<Campaign>("Campaigns");
            _paymentGateway = paymentGateway;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Donation> CreateDonationAsync(Donation donation)
        {
            try
            {
                // Validate donation
                if (donation.Amount <= 0)
                {
                    throw new ArgumentException("Donation amount must be greater than zero.");
                }

                // Set creation date
                donation.CreatedAt = DateTime.UtcNow;
                
                // Save the donation to database
                await _donations.InsertOneAsync(donation);

                // Update campaign's amount raised
                var update = Builders<Campaign>.Update.Inc(c => c.AmountRaised, donation.Amount);
                await _campaigns.UpdateOneAsync(c => c.Id == donation.CampaignId, update);

                // Send notification to campaign owner
                var campaign = await _campaigns.Find(c => c.Id == donation.CampaignId).FirstOrDefaultAsync();
                if (campaign != null)
                {
                    await _notificationService.SendNotificationAsync(
                        campaign.OrganizationId,
                        "New Donation Received",
                        $"You received a new donation of {donation.Amount:C} for {campaign.Title}."
                    );
                }

                // Check if any milestones have been reached
                await CheckMilestonesAsync(donation.CampaignId);

                return donation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donation: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Donation>> GetDonationsByCampaignAsync(string campaignId)
        {
            return await _donations.Find(d => d.CampaignId == campaignId).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetDonationsByUserAsync(string userId)
        {
            return await _donations.Find(d => d.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetDonationsByTransactionIdAsync(string transactionId)
        {
            try
            {
                return await _donations.Find(d => d.TransactionId == transactionId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting donations by transaction ID: {Message}", ex.Message);
                return new List<Donation>();
            }
        }

        public async Task<Donation> GetDonationByIdAsync(string id)
        {
            return await _donations.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateDonationStatusAsync(string id, PaymentStatus status)
        {
            var update = Builders<Donation>.Update
                .Set(d => d.PaymentStatus, status)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);
            
            var result = await _donations.UpdateOneAsync(d => d.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RefundDonationAsync(string id, string reason)
        {
            var donation = await _donations.Find(d => d.Id == id).FirstOrDefaultAsync();
            if (donation == null)
            {
                return false;
            }

            // Process refund through payment gateway
            var refundResult = await _paymentGateway.RefundPaymentAsync(donation.TransactionId, donation.Amount, reason);
            if (!refundResult.Success)
            {
                _logger.LogError("Failed to refund payment: {Message}", refundResult.ErrorMessage);
                return false;
            }

            // Update donation status
            var update = Builders<Donation>.Update
                .Set(d => d.PaymentStatus, PaymentStatus.Refunded)
                .Set(d => d.RefundedAt, DateTime.UtcNow)
                .Set(d => d.RefundReason, reason)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);
            
            await _donations.UpdateOneAsync(d => d.Id == id, update);

            // Update campaign's amount raised
            var campaignUpdate = Builders<Campaign>.Update.Inc(c => c.AmountRaised, -donation.Amount);
            await _campaigns.UpdateOneAsync(c => c.Id == donation.CampaignId, campaignUpdate);

            // Notify user about refund
            if (!string.IsNullOrEmpty(donation.UserId))
            {
                await _notificationService.SendNotificationAsync(
                    donation.UserId,
                    "Donation Refunded",
                    $"Your donation of {donation.Amount:C} has been refunded. Reason: {reason}"
                );
            }

            return true;
        }

        public async Task<Dictionary<string, decimal>> GetDonationStatisticsAsync(string campaignId)
        {
            var donations = await _donations.Find(d => d.CampaignId == campaignId && d.PaymentStatus == PaymentStatus.Completed).ToListAsync();
            
            var result = new Dictionary<string, decimal>
            {
                { "totalAmount", donations.Sum(d => d.Amount) },
                { "averageAmount", donations.Count > 0 ? donations.Average(d => d.Amount) : 0 },
                { "largestAmount", donations.Count > 0 ? donations.Max(d => d.Amount) : 0 },
                { "donorCount", donations.Select(d => d.UserId).Distinct().Count() }
            };

            return result;
        }

        public async Task<Dictionary<string, object>> GetDonationInsightsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var filterBuilder = Builders<Donation>.Filter;
            var filter = filterBuilder.Eq(d => d.PaymentStatus, PaymentStatus.Completed);

            if (startDate.HasValue)
            {
                filter &= filterBuilder.Gte(d => d.CreatedAt, startDate.Value);
            }

            if (endDate.HasValue)
            {
                filter &= filterBuilder.Lte(d => d.CreatedAt, endDate.Value);
            }

            var donations = await _donations.Find(filter).ToListAsync();
            
            // Group by day
            var donationsByDay = donations
                .GroupBy(d => d.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(d => d.Amount), Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            // Calculate growth rate
            decimal growthRate = 0;
            if (donations.Count > 0 && startDate.HasValue && endDate.HasValue)
            {
                var dayCount = (endDate.Value - startDate.Value).Days;
                if (dayCount > 0)
                {
                    var firstDayAmount = donationsByDay.FirstOrDefault()?.Amount ?? 0;
                    var lastDayAmount = donationsByDay.LastOrDefault()?.Amount ?? 0;
                    
                    if (firstDayAmount > 0)
                    {
                        growthRate = ((lastDayAmount / firstDayAmount) - 1) * 100;
                    }
                }
            }

            // Get top campaigns
            var topCampaigns = donations
                .GroupBy(d => d.CampaignId)
                .Select(g => new { CampaignId = g.Key, Amount = g.Sum(d => d.Amount) })
                .OrderByDescending(x => x.Amount)
                .Take(5)
                .ToList();

            return new Dictionary<string, object>
            {
                { "totalDonations", donations.Count },
                { "totalAmount", donations.Sum(d => d.Amount) },
                { "averageDonation", donations.Count > 0 ? donations.Average(d => d.Amount) : 0 },
                { "donationsByDay", donationsByDay },
                { "growthRate", growthRate },
                { "topCampaigns", topCampaigns },
                { "recurringDonationsCount", donations.Count(d => d.IsSubscriptionDonation) },
                { "recurringDonationsAmount", donations.Where(d => d.IsSubscriptionDonation).Sum(d => d.Amount) }
            };
        }

        public async Task<string> GenerateDonationReceiptAsync(string donationId)
        {
            var donation = await _donations.Find(d => d.Id == donationId).FirstOrDefaultAsync();
            if (donation == null)
            {
                throw new ArgumentException($"Donation with ID {donationId} not found.");
            }

            // In a real implementation, this would generate a PDF receipt and store it in a blob storage
            // For now, we'll just return a URL placeholder
            return $"https://raiseyourvoice.com/receipts/{donationId}.pdf";
        }

        public async Task<bool> CreateSubscriptionDonationAsync(string userId, string campaignId, decimal amount, string paymentMethodId)
        {
            try
            {
                // Get the user's donor information
                var existingDonation = await _donations.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                var donorInfo = existingDonation?.DonorInformation;
                
                if (donorInfo == null)
                {
                    throw new ArgumentException("User's donor information not found.");
                }

                // Get the campaign details
                var campaign = await _campaigns.Find(c => c.Id == campaignId).FirstOrDefaultAsync();
                if (campaign == null)
                {
                    throw new ArgumentException($"Campaign with ID {campaignId} not found.");
                }

                // Create a subscription through the payment gateway
                var subscriptionId = await _paymentGateway.CreateSubscriptionAsync(
                    userId, // Using userId as customerId for simplicity
                    paymentMethodId,
                    amount,
                    $"Monthly donation to {campaign.Title}"
                );

                // Create a donation record
                var donation = new Donation
                {
                    CampaignId = campaignId,
                    UserId = userId,
                    Amount = amount,
                    IsAnonymous = false,
                    PaymentStatus = PaymentStatus.Completed,
                    PaymentMethod = "card", // Assuming card payment
                    Currency = "USD",
                    IsSubscriptionDonation = true,
                    SubscriptionId = subscriptionId,
                    DonorInformation = donorInfo,
                    CreatedAt = DateTime.UtcNow
                };

                await _donations.InsertOneAsync(donation);

                // Update campaign's amount raised
                var update = Builders<Campaign>.Update.Inc(c => c.AmountRaised, amount);
                await _campaigns.UpdateOneAsync(c => c.Id == campaignId, update);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription donation: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> CancelSubscriptionDonationAsync(string subscriptionId)
        {
            try
            {
                // Cancel the subscription through the payment gateway
                var result = await _paymentGateway.CancelSubscriptionAsync(subscriptionId);
                if (!result)
                {
                    return false;
                }

                // Update the donation record
                var update = Builders<Donation>.Update
                    .Set(d => d.UpdatedAt, DateTime.UtcNow)
                    .Set(d => d.PaymentStatus, PaymentStatus.Cancelled);
                
                var updateResult = await _donations.UpdateOneAsync(d => d.SubscriptionId == subscriptionId, update);
                return updateResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling subscription donation: {Message}", ex.Message);
                return false;
            }
        }

        private async Task CheckMilestonesAsync(string campaignId)
        {
            var campaign = await _campaigns.Find(c => c.Id == campaignId).FirstOrDefaultAsync();
            if (campaign == null || campaign.Milestones == null || !campaign.Milestones.Any())
            {
                return;
            }

            foreach (var milestone in campaign.Milestones.Where(m => !m.IsCompleted && m.TargetAmount <= campaign.AmountRaised))
            {
                // Mark milestone as complete
                milestone.IsCompleted = true;
                milestone.ReachedAt = DateTime.UtcNow;
                
                // Notify organization
                await _notificationService.SendNotificationAsync(
                    campaign.OrganizationId,
                    "Campaign Milestone Reached",
                    $"Your campaign '{campaign.Title}' has reached the milestone of {milestone.TargetAmount:C}!"
                );
            }

            // Update the campaign with milestone changes
            var update = Builders<Campaign>.Update.Set(c => c.Milestones, campaign.Milestones);
            await _campaigns.UpdateOneAsync(c => c.Id == campaignId, update);
        }
    }
}