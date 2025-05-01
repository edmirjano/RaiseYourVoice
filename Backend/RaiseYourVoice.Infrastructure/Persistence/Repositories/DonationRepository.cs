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
            return await _collection.Find(d => d.CampaignId == campaignId).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(d => d.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByPaymentStatusAsync(PaymentStatus status)
        {
            return await _collection.Find(d => d.PaymentStatus == status).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByTransactionIdAsync(string transactionId)
        {
            return await _collection.Find(d => d.TransactionId == transactionId).ToListAsync();
        }

        public async Task<bool> UpdatePaymentStatusAsync(string id, PaymentStatus status)
        {
            var update = Builders<Donation>.Update
                .Set(d => d.PaymentStatus, status)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(d => d.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<Dictionary<string, decimal>> GetDonationStatisticsByCampaignAsync(string campaignId)
        {
            var donations = await _collection.Find(d => d.CampaignId == campaignId && d.PaymentStatus == PaymentStatus.Completed).ToListAsync();
            
            if (!donations.Any())
            {
                return new Dictionary<string, decimal>
                {
                    { "totalAmount", 0 },
                    { "averageAmount", 0 },
                    { "donorCount", 0 }
                };
            }

            decimal totalAmount = donations.Sum(d => d.Amount);
            int donorCount = donations.Select(d => d.UserId).Distinct().Count();
            decimal averageAmount = donorCount > 0 ? totalAmount / donorCount : 0;

            return new Dictionary<string, decimal>
            {
                { "totalAmount", totalAmount },
                { "averageAmount", averageAmount },
                { "donorCount", donorCount }
            };
        }

        public async Task<IEnumerable<Donation>> GetSubscriptionDonationsAsync(string subscriptionId)
        {
            return await _collection.Find(d => d.IsSubscriptionDonation && d.SubscriptionId == subscriptionId).ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetDonationInsightsAsync(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var filter = Builders<Donation>.Filter.And(
                Builders<Donation>.Filter.Gte(d => d.CreatedAt, start),
                Builders<Donation>.Filter.Lte(d => d.CreatedAt, end),
                Builders<Donation>.Filter.Eq(d => d.PaymentStatus, PaymentStatus.Completed)
            );

            var donations = await _collection.Find(filter).ToListAsync();

            // Group donations by day
            var donationsByDay = donations
                .GroupBy(d => d.CreatedAt.Date)
                .Select(g => new 
                { 
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count(),
                    Amount = g.Sum(d => d.Amount)
                })
                .OrderBy(g => g.Date)
                .ToList();

            // Calculate total amount, count, and average
            var totalAmount = donations.Sum(d => d.Amount);
            var totalCount = donations.Count;
            var averageAmount = totalCount > 0 ? totalAmount / totalCount : 0;

            // Calculate recurring vs one-time
            var recurringCount = donations.Count(d => d.IsSubscriptionDonation);
            var oneTimeCount = totalCount - recurringCount;

            return new Dictionary<string, object>
            {
                { "totalAmount", totalAmount },
                { "totalDonations", totalCount },
                { "averageDonation", averageAmount },
                { "recurringDonations", recurringCount },
                { "oneTimeDonations", oneTimeCount },
                { "donationsByDay", donationsByDay }
            };
        }
    }
}