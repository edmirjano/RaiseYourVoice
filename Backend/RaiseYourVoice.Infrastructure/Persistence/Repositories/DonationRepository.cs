using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class DonationRepository : MongoGenericRepository<Donation>
    {
        public DonationRepository(MongoDbContext context) : base(context, "Donations")
        {
        }

        public async Task<IEnumerable<Donation>> GetByCampaignIdAsync(string campaignId)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.CampaignId, campaignId);
            return await _collection.Find(filter)
                .SortByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByDonorIdAsync(string userId)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.DonorId, userId);
            return await _collection.Find(filter)
                .SortByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByStatusAsync(PaymentStatus status)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.Status, status);
            return await _collection.Find(filter)
                .SortByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalDonationsByCampaignAsync(string campaignId)
        {
            var filter = Builders<Donation>.Filter.And(
                Builders<Donation>.Filter.Eq(d => d.CampaignId, campaignId),
                Builders<Donation>.Filter.Eq(d => d.Status, PaymentStatus.Completed)
            );
            
            var donations = await _collection.Find(filter).ToListAsync();
            return donations.Sum(d => d.Amount);
        }

        public async Task<int> GetDonorCountByCampaignAsync(string campaignId)
        {
            var filter = Builders<Donation>.Filter.And(
                Builders<Donation>.Filter.Eq(d => d.CampaignId, campaignId),
                Builders<Donation>.Filter.Eq(d => d.Status, PaymentStatus.Completed)
            );
            
            return (int)(await _collection.DistinctAsync<string>(d => d.DonorId, filter)).ToList().Count;
        }

        public async Task<bool> UpdateDonationStatusAsync(string donationId, PaymentStatus status, string transactionId = null)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.Id, donationId);
            var update = Builders<Donation>.Update
                .Set(d => d.Status, status)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);
                
            if (!string.IsNullOrEmpty(transactionId))
            {
                update = update.Set(d => d.TransactionId, transactionId);
            }
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<Dictionary<string, decimal>> GetDonationStatsByPeriodAsync(string campaignId, DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Donation>.Filter.And(
                Builders<Donation>.Filter.Eq(d => d.CampaignId, campaignId),
                Builders<Donation>.Filter.Eq(d => d.Status, PaymentStatus.Completed),
                Builders<Donation>.Filter.Gte(d => d.CreatedAt, startDate),
                Builders<Donation>.Filter.Lt(d => d.CreatedAt, endDate)
            );
            
            var donations = await _collection.Find(filter).ToListAsync();
            
            // Group by date (day) and calculate sum for each day
            var stats = donations
                .GroupBy(d => d.CreatedAt.Date.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));
                
            return stats;
        }

        public async Task<IEnumerable<Donation>> GetRecentDonationsAsync(string campaignId, int limit = 5)
        {
            var filter = Builders<Donation>.Filter.And(
                Builders<Donation>.Filter.Eq(d => d.CampaignId, campaignId),
                Builders<Donation>.Filter.Eq(d => d.Status, PaymentStatus.Completed)
            );
            
            return await _collection.Find(filter)
                .SortByDescending(d => d.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }
    }
}