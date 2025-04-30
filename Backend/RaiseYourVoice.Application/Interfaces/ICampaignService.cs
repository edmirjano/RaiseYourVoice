using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Application.Interfaces
{
    public interface ICampaignService
    {
        Task<IEnumerable<Campaign>> GetAllCampaignsAsync();
        Task<IEnumerable<Campaign>> GetFeaturedCampaignsAsync();
        Task<IEnumerable<Campaign>> GetCampaignsByCategoryAsync(string category);
        Task<Campaign> GetCampaignByIdAsync(string id);
        Task<Campaign> CreateCampaignAsync(Campaign campaign);
        Task<Campaign> UpdateCampaignAsync(Campaign campaign);
        Task<bool> DeleteCampaignAsync(string id);
        Task<bool> ApproveCampaignAsync(string id);
        Task<bool> RejectCampaignAsync(string id, string reason);
        Task<bool> AddCampaignUpdateAsync(string campaignId, CampaignUpdate update);
        Task<bool> AddCampaignMilestoneAsync(string campaignId, CampaignMilestone milestone);
        Task<bool> UpdateTransparencyReportAsync(string campaignId, TransparencyReport report);
        Task<bool> FeatureCampaignAsync(string id, bool featured);
        Task<IEnumerable<Campaign>> SearchCampaignsAsync(string query);
        Task<Dictionary<string, decimal>> GetCampaignStatisticsAsync(string campaignId);
    }
}