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
    public class CampaignSeeder : BaseDataSeeder<Campaign>
    {
        private readonly IMongoCollection<Organization> _organizationCollection;
        
        public CampaignSeeder(MongoDbContext dbContext, ILogger<CampaignSeeder> logger) 
            : base(dbContext, logger, "Campaigns")
        {
            _organizationCollection = dbContext.Organizations;
        }

        protected override async Task SeedDataAsync()
        {
            // First, get organization IDs to associate with campaigns
            var organizations = await _organizationCollection.Find(Builders<Organization>.Filter.Empty)
                .Project(org => new { org.Id, org.Name })
                .ToListAsync();
                
            if (organizations.Count == 0)
            {
                _logger.LogWarning("No organizations found for associating with campaigns. Please seed organizations first.");
                return;
            }
            
            var campaigns = new List<Campaign>();
            var random = new Random();
            
            foreach (var org in organizations)
            {
                // Create 2 campaigns for each organization
                for (int i = 0; i < 2; i++)
                {
                    var isFeatured = i == 0; // First campaign for each org is featured
                    
                    var campaign = new Campaign
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = GetCampaignTitle(org.Name, i),
                        Description = GetCampaignDescription(i),
                        OrganizationId = org.Id,
                        Status = CampaignStatus.Active,
                        Category = GetRandomCategory(),
                        Goal = GetRandomGoal(),
                        AmountRaised = 0, // Will be populated by donation seeder
                        StartDate = DateTime.UtcNow.AddDays(-random.Next(5, 20)),
                        EndDate = DateTime.UtcNow.AddDays(random.Next(30, 90)),
                        IsFeatured = isFeatured,
                        CoverImage = $"https://storage.raiseyourvoice.al/campaigns/campaign-{i + 1}.webp",
                        ViewCount = random.Next(100, 1000),
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(20, 40)),
                        UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 5)),
                        Location = new GeoLocation
                        {
                            Type = "Point",
                            Coordinates = new double[] { 19.82 + random.NextDouble() * 0.1, 41.32 + random.NextDouble() * 0.1 }
                        },
                        Milestones = GenerateMilestones(),
                        Updates = GenerateUpdates(),
                        FundingUsage = "Funds will be used for program implementation, material procurement, and operational costs directly related to this initiative.",
                        ImpactDescription = "This campaign aims to create measurable positive change within our community by addressing key social and environmental challenges.",
                        RewardTiers = GenerateRewardTiers()
                    };
                    
                    campaigns.Add(campaign);
                }
            }
            
            await _collection.InsertManyAsync(campaigns);
            _logger.LogInformation("Inserted {Count} sample campaigns", campaigns.Count);
        }
        
        private string GetCampaignTitle(string orgName, int index)
        {
            if (orgName.Contains("Earth"))
            {
                return index == 0 
                    ? "Reforestation Project: 10,000 Trees for Albania" 
                    : "Plastic-Free Beaches Initiative";
            }
            else if (orgName.Contains("Youth"))
            {
                return index == 0 
                    ? "Digital Skills for Rural Youth" 
                    : "Youth Leadership Academy";
            }
            
            return $"Campaign {index + 1} for {orgName}";
        }
        
        private string GetCampaignDescription(int index)
        {
            var descriptions = new[]
            {
                "This campaign aims to restore Albania's forests by planting native tree species in areas affected by deforestation and wildfires. Our goal is to plant 10,000 trees across key ecological zones, improving air quality, preventing erosion, and creating habitats for wildlife. Join us in building a greener future for Albania!",
                
                "Help us clean up and protect Albania's beautiful beaches from plastic pollution. This initiative will organize regular beach cleanups, install recycling stations along major coastal areas, and conduct educational workshops for local communities and tourists about reducing plastic waste. Together, we can preserve our coastline for future generations.",
                
                "This program will provide digital skills training to young people in rural areas of Albania, helping bridge the urban-rural digital divide. Participants will learn coding, digital marketing, and entrepreneurship skills that can help them access remote work opportunities or start online businesses. Your support will fund laptops, internet access, and expert trainers.",
                
                "The Youth Leadership Academy provides comprehensive leadership training, mentorship, and networking opportunities for promising young activists and community leaders. Through workshops, project implementation, and guidance from established leaders, participants develop the skills needed to create positive change in their communities."
            };
            
            return descriptions[Math.Min(index, descriptions.Length - 1)];
        }
        
        private CampaignCategory GetRandomCategory()
        {
            var categories = Enum.GetValues(typeof(CampaignCategory)).Cast<CampaignCategory>().ToArray();
            return categories[new Random().Next(0, categories.Length)];
        }
        
        private decimal GetRandomGoal()
        {
            var goals = new[] { 5000m, 7500m, 10000m, 15000m, 25000m };
            return goals[new Random().Next(0, goals.Length)];
        }
        
        private List<CampaignMilestone> GenerateMilestones()
        {
            var milestones = new List<CampaignMilestone>();
            var random = new Random();
            
            for (int i = 0; i < 3; i++)
            {
                milestones.Add(new CampaignMilestone
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"Milestone {i + 1}",
                    Description = $"This is milestone {i + 1} for the campaign.",
                    TargetAmount = (i + 1) * 2500m,
                    IsCompleted = false,
                    Order = i + 1
                });
            }
            
            return milestones;
        }
        
        private List<CampaignUpdate> GenerateUpdates()
        {
            var updates = new List<CampaignUpdate>();
            var random = new Random();
            
            for (int i = 0; i < 2; i++)
            {
                updates.Add(new CampaignUpdate
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"Campaign Update {i + 1}",
                    Content = $"This is update {i + 1} for the campaign with the latest progress and achievements.",
                    PostedAt = DateTime.UtcNow.AddDays(-random.Next(1, 10)),
                    ImageUrls = new List<string> { $"https://storage.raiseyourvoice.al/campaigns/updates/update-{i + 1}.webp" }
                });
            }
            
            return updates;
        }
        
        private List<RewardTier> GenerateRewardTiers()
        {
            return new List<RewardTier>
            {
                new RewardTier
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Supporter",
                    Description = "Thank you for your support! You'll receive a digital certificate and regular updates.",
                    MinimumAmount = 10m,
                    MaximumAmount = 49m
                },
                new RewardTier
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Advocate",
                    Description = "All Supporter benefits plus recognition on our website and a campaign sticker.",
                    MinimumAmount = 50m,
                    MaximumAmount = 99m
                },
                new RewardTier
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Champion",
                    Description = "All Advocate benefits plus a campaign t-shirt and invitation to our annual event.",
                    MinimumAmount = 100m,
                    MaximumAmount = null
                }
            };
        }
    }
}