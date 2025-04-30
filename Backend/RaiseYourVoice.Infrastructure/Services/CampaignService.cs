using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IMongoCollection<Campaign> _campaigns;
        private readonly IMongoCollection<Organization> _organizations;
        private readonly IPushNotificationService _notificationService;
        private readonly ILogger<CampaignService> _logger;

        public CampaignService(
            IMongoClient mongoClient,
            IPushNotificationService notificationService,
            ILogger<CampaignService> logger)
        {
            var database = mongoClient.GetDatabase("RaiseYourVoice");
            _campaigns = database.GetCollection<Campaign>("Campaigns");
            _organizations = database.GetCollection<Organization>("Organizations");
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<Campaign>> GetAllCampaignsAsync()
        {
            return await _campaigns.Find(c => c.Status == CampaignStatus.Active).ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetFeaturedCampaignsAsync()
        {
            return await _campaigns.Find(c => c.Status == CampaignStatus.Active && c.IsFeatured).ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetCampaignsByCategoryAsync(string category)
        {
            if (!Enum.TryParse<CampaignCategory>(category, true, out var campaignCategory))
            {
                throw new ArgumentException($"Invalid campaign category: {category}");
            }
            
            return await _campaigns.Find(c => c.Status == CampaignStatus.Active && c.Category == campaignCategory).ToListAsync();
        }

        public async Task<Campaign> GetCampaignByIdAsync(string id)
        {
            // Increment view count when campaign is viewed
            var update = Builders<Campaign>.Update.Inc(c => c.ViewCount, 1);
            await _campaigns.UpdateOneAsync(c => c.Id == id, update);
            
            return await _campaigns.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Campaign> CreateCampaignAsync(Campaign campaign)
        {
            try
            {
                // Validate campaign
                if (string.IsNullOrEmpty(campaign.Title))
                {
                    throw new ArgumentException("Campaign title is required.");
                }

                if (campaign.Goal <= 0)
                {
                    throw new ArgumentException("Campaign goal must be greater than zero.");
                }

                if (campaign.EndDate <= campaign.StartDate)
                {
                    throw new ArgumentException("End date must be later than start date.");
                }

                // Verify the organization exists
                var organization = await _organizations.Find(o => o.Id == campaign.OrganizationId).FirstOrDefaultAsync();
                if (organization == null)
                {
                    throw new ArgumentException($"Organization with ID {campaign.OrganizationId} not found.");
                }

                // Set default values
                campaign.CreatedAt = DateTime.UtcNow;
                campaign.AmountRaised = 0;
                campaign.ViewCount = 0;
                campaign.Status = CampaignStatus.PendingApproval;
                campaign.IsFeatured = false;

                // Save the campaign
                await _campaigns.InsertOneAsync(campaign);

                // Notify administrators about new campaign for approval
                // This would typically go to admin users with specific roles
                await _notificationService.SendAdminNotificationAsync(
                    "New Campaign Submission",
                    $"A new campaign '{campaign.Title}' has been submitted for approval by {organization.Name}."
                );

                return campaign;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campaign: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Campaign> UpdateCampaignAsync(Campaign campaign)
        {
            try
            {
                // Validate the campaign exists
                var existingCampaign = await _campaigns.Find(c => c.Id == campaign.Id).FirstOrDefaultAsync();
                if (existingCampaign == null)
                {
                    throw new ArgumentException($"Campaign with ID {campaign.Id} not found.");
                }

                // Don't allow changing certain fields
                campaign.CreatedAt = existingCampaign.CreatedAt;
                campaign.AmountRaised = existingCampaign.AmountRaised;
                campaign.Status = existingCampaign.Status;
                campaign.IsFeatured = existingCampaign.IsFeatured;
                campaign.ViewCount = existingCampaign.ViewCount;
                
                // Set update time
                campaign.UpdatedAt = DateTime.UtcNow;

                // Replace the campaign document
                await _campaigns.ReplaceOneAsync(c => c.Id == campaign.Id, campaign);

                // If the campaign is active, notify donors about the update
                if (campaign.Status == CampaignStatus.Active)
                {
                    // This would typically trigger an email notification rather than a push notification
                    // But for simplicity we'll use the push notification service
                    // In a real system, you'd use a mailing service for this
                    await _notificationService.SendCampaignUpdateNotificationAsync(
                        campaign.Id,
                        "Campaign Updated",
                        $"The campaign '{campaign.Title}' has been updated. Check out the changes!"
                    );
                }

                return campaign;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating campaign: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteCampaignAsync(string id)
        {
            try
            {
                // First check if the campaign exists
                var campaign = await _campaigns.Find(c => c.Id == id).FirstOrDefaultAsync();
                if (campaign == null)
                {
                    return false;
                }

                // Check if the campaign has donations
                if (campaign.AmountRaised > 0)
                {
                    // If it has donations, don't delete it - just mark it as canceled
                    var update = Builders<Campaign>.Update
                        .Set(c => c.Status, CampaignStatus.Cancelled)
                        .Set(c => c.UpdatedAt, DateTime.UtcNow);
                    
                    await _campaigns.UpdateOneAsync(c => c.Id == id, update);
                    return true;
                }

                // If no donations, we can delete it
                var result = await _campaigns.DeleteOneAsync(c => c.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting campaign: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> ApproveCampaignAsync(string id)
        {
            try
            {
                var update = Builders<Campaign>.Update
                    .Set(c => c.Status, CampaignStatus.Active)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
                var result = await _campaigns.UpdateOneAsync(c => c.Id == id && c.Status == CampaignStatus.PendingApproval, update);
                
                if (result.ModifiedCount > 0)
                {
                    // Get the campaign and organization details
                    var campaign = await _campaigns.Find(c => c.Id == id).FirstOrDefaultAsync();
                    
                    // Notify organization about approval
                    await _notificationService.SendNotificationAsync(
                        campaign.OrganizationId,
                        "Campaign Approved",
                        $"Your campaign '{campaign.Title}' has been approved and is now live!"
                    );
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving campaign: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> RejectCampaignAsync(string id, string reason)
        {
            try
            {
                var update = Builders<Campaign>.Update
                    .Set(c => c.Status, CampaignStatus.Rejected)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
                var result = await _campaigns.UpdateOneAsync(c => c.Id == id && c.Status == CampaignStatus.PendingApproval, update);
                
                if (result.ModifiedCount > 0)
                {
                    // Get the campaign details
                    var campaign = await _campaigns.Find(c => c.Id == id).FirstOrDefaultAsync();
                    
                    // Notify organization about rejection
                    await _notificationService.SendNotificationAsync(
                        campaign.OrganizationId,
                        "Campaign Rejected",
                        $"Your campaign '{campaign.Title}' has been rejected. Reason: {reason}"
                    );
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting campaign: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> AddCampaignUpdateAsync(string campaignId, CampaignUpdate update)
        {
            try
            {
                if (string.IsNullOrEmpty(update.Title) || string.IsNullOrEmpty(update.Content))
                {
                    throw new ArgumentException("Update title and content are required.");
                }

                // Generate an ID for the update if not provided
                if (string.IsNullOrEmpty(update.Id))
                {
                    update.Id = Guid.NewGuid().ToString();
                }

                update.CampaignId = campaignId;
                update.PostedAt = DateTime.UtcNow;

                // Add the update to the campaign
                var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
                var updateDefinition = Builders<Campaign>.Update.Push(c => c.Updates, update);
                
                var result = await _campaigns.UpdateOneAsync(filter, updateDefinition);
                
                if (result.ModifiedCount > 0)
                {
                    // Get the campaign details
                    var campaign = await _campaigns.Find(c => c.Id == campaignId).FirstOrDefaultAsync();
                    
                    // Notify donors about the update
                    await _notificationService.SendCampaignUpdateNotificationAsync(
                        campaignId,
                        "Campaign Update",
                        $"New update for '{campaign.Title}': {update.Title}"
                    );
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding campaign update: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> AddCampaignMilestoneAsync(string campaignId, CampaignMilestone milestone)
        {
            try
            {
                if (string.IsNullOrEmpty(milestone.Title) || milestone.TargetAmount <= 0)
                {
                    throw new ArgumentException("Milestone title and target amount are required.");
                }

                // Generate an ID for the milestone if not provided
                if (string.IsNullOrEmpty(milestone.Id))
                {
                    milestone.Id = Guid.NewGuid().ToString();
                }

                milestone.IsCompleted = false;

                // Add the milestone to the campaign
                var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
                var update = Builders<Campaign>.Update.Push(c => c.Milestones, milestone);
                
                var result = await _campaigns.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding campaign milestone: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateTransparencyReportAsync(string campaignId, TransparencyReport report)
        {
            try
            {
                if (string.IsNullOrEmpty(report.Id))
                {
                    report.Id = Guid.NewGuid().ToString();
                }

                report.CampaignId = campaignId;
                report.LastUpdated = DateTime.UtcNow;

                // Update the transparency report
                var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
                var update = Builders<Campaign>.Update
                    .Set(c => c.TransparencyReport, report)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
                var result = await _campaigns.UpdateOneAsync(filter, update);
                
                if (result.ModifiedCount > 0)
                {
                    // Get the campaign details
                    var campaign = await _campaigns.Find(c => c.Id == campaignId).FirstOrDefaultAsync();
                    
                    // Notify donors about the transparency report update
                    await _notificationService.SendCampaignUpdateNotificationAsync(
                        campaignId,
                        "Transparency Report Updated",
                        $"The transparency report for '{campaign.Title}' has been updated. Check it out to see how your donation is being used."
                    );
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transparency report: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> FeatureCampaignAsync(string id, bool featured)
        {
            try
            {
                var update = Builders<Campaign>.Update
                    .Set(c => c.IsFeatured, featured)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
                var result = await _campaigns.UpdateOneAsync(c => c.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error featuring campaign: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<IEnumerable<Campaign>> SearchCampaignsAsync(string query)
        {
            try
            {
                // Simple text search on title and description
                var filter = Builders<Campaign>.Filter.Where(c => 
                    c.Status == CampaignStatus.Active && 
                    (c.Title.ToLower().Contains(query.ToLower()) || 
                     c.Description.ToLower().Contains(query.ToLower()))
                );
                
                return await _campaigns.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching campaigns: {Message}", ex.Message);
                return Enumerable.Empty<Campaign>();
            }
        }

        public async Task<Dictionary<string, decimal>> GetCampaignStatisticsAsync(string campaignId)
        {
            try
            {
                var campaign = await _campaigns.Find(c => c.Id == campaignId).FirstOrDefaultAsync();
                if (campaign == null)
                {
                    throw new ArgumentException($"Campaign with ID {campaignId} not found.");
                }

                // Calculate days remaining
                var daysRemaining = (campaign.EndDate - DateTime.UtcNow).Days;
                daysRemaining = daysRemaining < 0 ? 0 : daysRemaining;

                // Calculate progress percentage
                var progressPercentage = campaign.Goal > 0 
                    ? Math.Min(100, (campaign.AmountRaised / campaign.Goal) * 100) 
                    : 0;

                return new Dictionary<string, decimal>
                {
                    { "goal", campaign.Goal },
                    { "amountRaised", campaign.AmountRaised },
                    { "progressPercentage", progressPercentage },
                    { "daysRemaining", daysRemaining },
                    { "viewCount", campaign.ViewCount },
                    { "donorCount", campaign.Donations?.Count ?? 0 },
                    { "averageDonation", campaign.Donations?.Count > 0 ? campaign.AmountRaised / campaign.Donations.Count : 0 }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign statistics: {Message}", ex.Message);
                throw;
            }
        }
    }
}