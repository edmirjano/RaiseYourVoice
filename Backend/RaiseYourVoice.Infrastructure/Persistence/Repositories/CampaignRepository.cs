using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using RaiseYourVoice.Infrastructure.Persistence;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class CampaignRepository : MongoGenericRepository<Campaign>
    {
        public CampaignRepository(MongoDbContext context) : base(context, "Campaigns")
        {
        }

        public async Task<IEnumerable<Campaign>> GetByOrganizationIdAsync(string organizationId)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.OrganizationId, organizationId);
            return await _collection.Find(filter)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetByStatusAsync(CampaignStatus status, int limit = 20, int skip = 0)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Status, status);
            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync(int limit = 20, int skip = 0)
        {
            var now = DateTime.UtcNow;
            var filter = Builders<Campaign>.Filter.And(
                Builders<Campaign>.Filter.Eq(c => c.Status, CampaignStatus.Active),
                Builders<Campaign>.Filter.Lte(c => c.StartDate, now),
                Builders<Campaign>.Filter.Gte(c => c.EndDate, now)
            );

            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortBy(c => c.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetFeaturedCampaignsAsync(int limit = 5)
        {
            var now = DateTime.UtcNow;
            var filter = Builders<Campaign>.Filter.And(
                Builders<Campaign>.Filter.Eq(c => c.Status, CampaignStatus.Active),
                Builders<Campaign>.Filter.Eq(c => c.IsFeatured, true),
                Builders<Campaign>.Filter.Lte(c => c.StartDate, now),
                Builders<Campaign>.Filter.Gte(c => c.EndDate, now)
            );

            return await _collection.Find(filter)
                .Limit(limit)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetEndingSoonCampaignsAsync(int daysThreshold = 7, int limit = 10)
        {
            var now = DateTime.UtcNow;
            var thresholdDate = now.AddDays(daysThreshold);
            
            var filter = Builders<Campaign>.Filter.And(
                Builders<Campaign>.Filter.Eq(c => c.Status, CampaignStatus.Active),
                Builders<Campaign>.Filter.Lte(c => c.StartDate, now),
                Builders<Campaign>.Filter.Gt(c => c.EndDate, now),
                Builders<Campaign>.Filter.Lte(c => c.EndDate, thresholdDate)
            );

            return await _collection.Find(filter)
                .Limit(limit)
                .SortBy(c => c.EndDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateFundingProgress(string campaignId, decimal amountRaised)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Inc(c => c.AmountRaised, amountRaised)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateCampaignStatusAsync(string campaignId, CampaignStatus status)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Set(c => c.Status, status)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> SetFeaturedStatusAsync(string campaignId, bool isFeatured)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Set(c => c.IsFeatured, isFeatured)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Campaign>> SearchCampaignsAsync(string searchTerm, int limit = 20)
        {
            var filter = Builders<Campaign>.Filter.Or(
                Builders<Campaign>.Filter.Regex(c => c.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Campaign>.Filter.Regex(c => c.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            
            return await _collection.Find(filter)
                .Limit(limit)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}