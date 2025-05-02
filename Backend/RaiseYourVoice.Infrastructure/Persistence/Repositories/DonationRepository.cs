using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class DonationRepository : MongoRepository<Donation>
    {
        public DonationRepository(MongoDbContext context, ILogger<DonationRepository> logger)
            : base(context, "Donations", logger)
        {
        }

        public async Task<IEnumerable<Donation>> GetByCampaignIdAsync(string campaignId)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.CampaignId, campaignId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByPaymentStatusAsync(PaymentStatus status)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.PaymentStatus, status);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByTransactionIdAsync(string transactionId)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.TransactionId, transactionId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetSubscriptionDonationsAsync()
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.IsSubscriptionDonation, true);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetBySubscriptionIdAsync(string subscriptionId)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.SubscriptionId, subscriptionId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> UpdateDonationStatusAsync(string donationId, PaymentStatus status)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.Id, donationId);
            var update = Builders<Donation>.Update
                .Set(d => d.PaymentStatus, status)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> RefundDonationAsync(string donationId, string reason)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.Id, donationId);
            var update = Builders<Donation>.Update
                .Set(d => d.PaymentStatus, PaymentStatus.Refunded)
                .Set(d => d.RefundedAt, DateTime.UtcNow)
                .Set(d => d.RefundReason, reason)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<Dictionary<string, decimal>> GetDonationStatisticsAsync(string campaignId)
        {
            try
            {
                var pipeline = new[]
                {
                    new BsonDocument("$match", new BsonDocument
                    {
                        { "CampaignId", campaignId },
                        { "PaymentStatus", PaymentStatus.Completed.ToString() }
                    }),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", null },
                        { "totalAmount", new BsonDocument("$sum", "$Amount") },
                        { "averageDonation", new BsonDocument("$avg", "$Amount") },
                        { "count", new BsonDocument("$sum", 1) }
                    })
                };

                var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                if (result != null)
                {
                    return new Dictionary<string, decimal>
                    {
                        { "totalAmount", result["totalAmount"].AsDecimal },
                        { "averageDonation", result["averageDonation"].AsDecimal },
                        { "count", result["count"].ToInt32() }
                    };
                }

                return new Dictionary<string, decimal>
                {
                    { "totalAmount", 0 },
                    { "averageDonation", 0 },
                    { "count", 0 }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting donation statistics for campaign {CampaignId}", campaignId);
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, object>> GetDonationInsightsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var matchFilter = new BsonDocument
                {
                    { "PaymentStatus", PaymentStatus.Completed.ToString() }
                };

                if (startDate.HasValue)
                {
                    matchFilter.Add("CreatedAt", new BsonDocument("$gte", startDate.Value));
                }

                if (endDate.HasValue)
                {
                    matchFilter.Add("CreatedAt", new BsonDocument("$lte", endDate.Value));
                }

                var pipeline = new[]
                {
                    new BsonDocument("$match", matchFilter),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", null },
                        { "totalDonations", new BsonDocument("$sum", 1) },
                        { "totalAmount", new BsonDocument("$sum", "$Amount") },
                        { "averageDonation", new BsonDocument("$avg", "$Amount") },
                        { "anonymousCount", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { "$IsAnonymous", 1, 0 })) }
                    })
                };

                var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                if (result != null)
                {
                    return new Dictionary<string, object>
                    {
                        { "totalDonations", result["totalDonations"].AsInt32 },
                        { "totalAmount", result["totalAmount"].AsDecimal },
                        { "averageDonation", result["averageDonation"].AsDecimal },
                        { "anonymousPercentage", (double)result["anonymousCount"].AsInt32 / result["totalDonations"].AsInt32 * 100 }
                    };
                }

                return new Dictionary<string, object>
                {
                    { "totalDonations", 0 },
                    { "totalAmount", 0m },
                    { "averageDonation", 0m },
                    { "anonymousPercentage", 0.0 }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting donation insights");
                return new Dictionary<string, object>();
            }
        }

        public async Task<IEnumerable<Donation>> GetPaginatedDonationsAsync(
            int pageNumber, 
            int pageSize, 
            string? campaignId = null, 
            string? userId = null, 
            PaymentStatus? status = null,
            bool? isAnonymous = null,
            bool? isSubscription = null,
            string? sortBy = null,
            bool ascending = true)
        {
            var filterBuilder = Builders<Donation>.Filter;
            var filters = new List<FilterDefinition<Donation>>();

            // Apply filters
            if (!string.IsNullOrEmpty(campaignId))
            {
                filters.Add(filterBuilder.Eq(d => d.CampaignId, campaignId));
            }

            if (!string.IsNullOrEmpty(userId))
            {
                filters.Add(filterBuilder.Eq(d => d.UserId, userId));
            }

            if (status.HasValue)
            {
                filters.Add(filterBuilder.Eq(d => d.PaymentStatus, status.Value));
            }

            if (isAnonymous.HasValue)
            {
                filters.Add(filterBuilder.Eq(d => d.IsAnonymous, isAnonymous.Value));
            }

            if (isSubscription.HasValue)
            {
                filters.Add(filterBuilder.Eq(d => d.IsSubscriptionDonation, isSubscription.Value));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Create sort definition
            SortDefinition<Donation> sortDefinition;

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Map sortBy string to property expression
                switch (sortBy.ToLower())
                {
                    case "amount":
                        sortDefinition = ascending 
                            ? Builders<Donation>.Sort.Ascending(d => d.Amount) 
                            : Builders<Donation>.Sort.Descending(d => d.Amount);
                        break;
                    case "date":
                        sortDefinition = ascending 
                            ? Builders<Donation>.Sort.Ascending(d => d.CreatedAt) 
                            : Builders<Donation>.Sort.Descending(d => d.CreatedAt);
                        break;
                    default:
                        sortDefinition = Builders<Donation>.Sort.Descending(d => d.CreatedAt);
                        break;
                }
            }
            else
            {
                // Default sort by created date descending
                sortDefinition = Builders<Donation>.Sort.Descending(d => d.CreatedAt);
            }

            return await _collection
                .Find(combinedFilter)
                .Sort(sortDefinition)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountDonationsAsync(
            string? campaignId = null,
            string? userId = null,
            PaymentStatus? status = null,
            bool? isAnonymous = null,
            bool? isSubscription = null)
        {
            var filterBuilder = Builders<Donation>.Filter;
            var filters = new List<FilterDefinition<Donation>>();

            // Apply filters
            if (!string.IsNullOrEmpty(campaignId))
            {
                filters.Add(filterBuilder.Eq(d => d.CampaignId, campaignId));
            }

            if (!string.IsNullOrEmpty(userId))
            {
                filters.Add(filterBuilder.Eq(d => d.UserId, userId));
            }

            if (status.HasValue)
            {
                filters.Add(filterBuilder.Eq(d => d.PaymentStatus, status.Value));
            }

            if (isAnonymous.HasValue)
            {
                filters.Add(filterBuilder.Eq(d => d.IsAnonymous, isAnonymous.Value));
            }

            if (isSubscription.HasValue)
            {
                filters.Add(filterBuilder.Eq(d => d.IsSubscriptionDonation, isSubscription.Value));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return await _collection.CountDocumentsAsync(combinedFilter);
        }
    }
}