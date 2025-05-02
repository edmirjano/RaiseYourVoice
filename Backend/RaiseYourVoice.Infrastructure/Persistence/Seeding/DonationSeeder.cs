using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Seeding
{
    public class DonationSeeder : BaseDataSeeder<Donation>
    {
        private readonly IMongoCollection<Campaign> _campaignCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly CampaignRepository _campaignRepository;
        
        public DonationSeeder(
            MongoDbContext dbContext, 
            ILogger<DonationSeeder> logger,
            CampaignRepository campaignRepository) 
            : base(dbContext, logger, "Donations")
        {
            _campaignCollection = dbContext.Campaigns;
            _userCollection = dbContext.Users;
            _campaignRepository = campaignRepository;
        }

        protected override async Task SeedDataAsync()
        {
            // First, get campaign IDs to associate with donations
            var campaigns = await _campaignCollection.Find(Builders<Campaign>.Filter.Empty)
                .Project(c => new { c.Id, c.Goal })
                .ToListAsync();
                
            if (campaigns.Count == 0)
            {
                _logger.LogWarning("No campaigns found for associating with donations. Please seed campaigns first.");
                return;
            }
            
            // Get users to associate as donors
            var users = await _userCollection.Find(Builders<User>.Filter.Empty)
                .Project(u => new { u.Id, u.Name, u.Email })
                .ToListAsync();
                
            if (users.Count == 0)
            {
                _logger.LogWarning("No users found for associating with donations. Please seed users first.");
                return;
            }
            
            var donations = new List<Donation>();
            var random = new Random();
            
            // For each campaign, create several donations
            foreach (var campaign in campaigns)
            {
                decimal totalForCampaign = 0;
                int donationCount = random.Next(5, 15); // Random number of donations per campaign
                
                for (int i = 0; i < donationCount; i++)
                {
                    // Select a random user as the donor
                    var user = users[random.Next(users.Count)];
                    
                    // Create a donation
                    var amount = GetRandomDonationAmount();
                    totalForCampaign += amount;
                    
                    var donation = new Donation
                    {
                        Id = Guid.NewGuid().ToString(),
                        CampaignId = campaign.Id,
                        UserId = user.Id,
                        Amount = amount,
                        PaymentStatus = PaymentStatus.Completed,
                        TransactionId = Guid.NewGuid().ToString(),
                        PaymentMethod = GetRandomPaymentMethod(),
                        IsAnonymous = random.Next(100) < 20, // 20% chance of anonymous donation
                        Message = GetRandomDonationMessage(),
                        IsSubscriptionDonation = random.Next(100) < 10, // 10% chance of subscription
                        SubscriptionId = random.Next(100) < 10 ? Guid.NewGuid().ToString() : null,
                        SelectedRewardTierId = amount >= 100 ? "champion_tier" : 
                                              amount >= 50 ? "advocate_tier" : 
                                              amount >= 10 ? "supporter_tier" : null,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 5))
                    };
                    
                    donations.Add(donation);
                }
                
                // Update the campaign's amount raised
                await _campaignRepository.UpdateAmountRaisedAsync(campaign.Id, totalForCampaign);
                _logger.LogInformation("Updated campaign {CampaignId} with total donations of {Amount}", 
                    campaign.Id, totalForCampaign);
            }
            
            await _collection.InsertManyAsync(donations);
            _logger.LogInformation("Inserted {Count} sample donations", donations.Count);
        }
        
        private decimal GetRandomDonationAmount()
        {
            var amounts = new[] { 10m, 25m, 50m, 100m, 250m, 500m };
            var random = new Random();
            return amounts[random.Next(amounts.Length)];
        }
        
        private PaymentMethod GetRandomPaymentMethod()
        {
            var methods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().ToArray();
            return methods[new Random().Next(methods.Length)];
        }
        
        private string GetRandomDonationMessage()
        {
            var messages = new[]
            {
                "Keep up the great work! This cause is so important.",
                "Happy to support this important initiative!",
                "Wishing you success with this amazing project.",
                "This is such an important cause. Good luck!",
                "Thank you for the work you're doing!",
                null, // Some donations have no message
                null
            };
            
            return messages[new Random().Next(messages.Length)];
        }
    }
}