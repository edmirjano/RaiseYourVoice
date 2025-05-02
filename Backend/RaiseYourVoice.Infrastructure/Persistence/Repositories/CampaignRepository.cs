using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using RaiseYourVoice.Application.Models.Pagination;
using System.Linq.Expressions;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class CampaignRepository : MongoRepository<Campaign>
    {
        public CampaignRepository(MongoDbContext context, ILogger<CampaignRepository> logger) 
            : base(context, "Campaigns", logger)
        {
        }

        public async Task<IEnumerable<Campaign>> GetByOrganizationIdAsync(string organizationId)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.OrganizationId, organizationId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetByStatusAsync(CampaignStatus status)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Status, status);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync()
        {
            var currentDate = DateTime.UtcNow;
            var filter = Builders<Campaign>.Filter.And(
                Builders<Campaign>.Filter.Eq(c => c.Status, CampaignStatus.Active),
                Builders<Campaign>.Filter.Lte(c => c.StartDate, currentDate),
                Builders<Campaign>.Filter.Gte(c => c.EndDate, currentDate)
            );
            
            return await _collection.Find(filter)
                .Sort(Builders<Campaign>.Sort.Ascending(c => c.EndDate))
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetFeaturedCampaignsAsync(int limit = 5)
        {
            var filter = Builders<Campaign>.Filter.And(
                Builders<Campaign>.Filter.Eq(c => c.Status, CampaignStatus.Active),
                Builders<Campaign>.Filter.Eq(c => c.IsFeatured, true)
            );
            
            return await _collection.Find(filter)
                .Sort(Builders<Campaign>.Sort.Descending(c => c.AmountRaised / c.Goal))
                .Limit(limit)
                .ToListAsync();
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

        public async Task<bool> UpdateFeaturedStatusAsync(string campaignId, bool isFeatured)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Set(c => c.IsFeatured, isFeatured)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> IncrementViewCountAsync(string campaignId)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Inc(c => c.ViewCount, 1);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateAmountRaisedAsync(string campaignId, decimal amount)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Set(c => c.AmountRaised, amount)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> IncrementAmountRaisedAsync(string campaignId, decimal amount)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Inc(c => c.AmountRaised, amount)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DecrementAmountRaisedAsync(string campaignId, decimal amount)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Inc(c => c.AmountRaised, -amount)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> AddCampaignUpdateAsync(string campaignId, CampaignUpdate update)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var updateCmd = Builders<Campaign>.Update
                .Push(c => c.Updates, update)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, updateCmd);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> AddCampaignMilestoneAsync(string campaignId, CampaignMilestone milestone)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Push(c => c.Milestones, milestone)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateCampaignMilestoneAsync(string campaignId, CampaignMilestone milestone)
        {
            var filter = Builders<Campaign>.Filter.And(
                Builders<Campaign>.Filter.Eq(c => c.Id, campaignId),
                Builders<Campaign>.Filter.ElemMatch(c => c.Milestones, m => m.Id == milestone.Id)
            );
            
            var update = Builders<Campaign>.Update
                .Set(c => c.Milestones[-1], milestone)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> AddExpenseItemAsync(string campaignId, ExpenseItem expense)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            
            // Check if TransparencyReport exists
            var campaign = await _collection.Find(filter).FirstOrDefaultAsync();
            
            UpdateDefinition<Campaign> update;
            
            if (campaign?.TransparencyReport == null)
            {
                // Create a new TransparencyReport
                var transparencyReport = new TransparencyReport
                {
                    Id = Guid.NewGuid().ToString(),
                    CampaignId = campaignId,
                    LastUpdated = DateTime.UtcNow,
                    Expenses = new List<ExpenseItem> { expense },
                    ReceiptUrls = new List<string>()
                };
                
                update = Builders<Campaign>.Update
                    .Set(c => c.TransparencyReport, transparencyReport)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);
            }
            else
            {
                // Add expense to existing report
                update = Builders<Campaign>.Update
                    .Push(c => c.TransparencyReport.Expenses, expense)
                    .Set(c => c.TransparencyReport.LastUpdated, DateTime.UtcNow)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);
            }
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateTransparencyReportAsync(string campaignId, TransparencyReport report)
        {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Id, campaignId);
            var update = Builders<Campaign>.Update
                .Set(c => c.TransparencyReport, report)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Campaign>> SearchCampaignsAsync(
            string searchText,
            CampaignCategory? category = null,
            CampaignStatus? status = null,
            string? organizationId = null)
        {
            var filterBuilder = Builders<Campaign>.Filter;
            var filters = new List<FilterDefinition<Campaign>>();
            
            // Add text search filter
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(c => c.Title, new MongoDB.Bson.BsonRegularExpression(searchText, "i")),
                filterBuilder.Regex(c => c.Description, new MongoDB.Bson.BsonRegularExpression(searchText, "i"))
            );
            filters.Add(searchFilter);
            
            // Add additional filters
            if (category.HasValue)
            {
                filters.Add(filterBuilder.Eq(c => c.Category, category.Value));
            }
            
            if (status.HasValue)
            {
                filters.Add(filterBuilder.Eq(c => c.Status, status.Value));
            }
            
            if (!string.IsNullOrEmpty(organizationId))
            {
                filters.Add(filterBuilder.Eq(c => c.OrganizationId, organizationId));
            }
            
            var combinedFilter = filterBuilder.And(filters);
            
            return await _collection.Find(combinedFilter)
                .Sort(Builders<Campaign>.Sort.Descending(c => c.CreatedAt))
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetCampaignsByLocationAsync(
            double latitude, 
            double longitude, 
            double maxDistanceKm = 50)
        {
            var point = new BsonDocument
            {
                { "type", "Point" },
                { "coordinates", new BsonArray { longitude, latitude } }
            };
            
            var filter = Builders<Campaign>.Filter.NearSphere(
                c => c.Location.Coordinates, 
                longitude, 
                latitude, 
                maxDistanceKm * 1000 // convert to meters
            );
            
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetPaginatedCampaignsAsync(
            int pageNumber,
            int pageSize,
            CampaignStatus? status = null,
            CampaignCategory? category = null,
            string? organizationId = null,
            bool? isFeatured = null,
            string? sortBy = null,
            bool ascending = false)
        {
            var filterBuilder = Builders<Campaign>.Filter;
            var filters = new List<FilterDefinition<Campaign>>();

            // Apply filters
            if (status.HasValue)
            {
                filters.Add(filterBuilder.Eq(c => c.Status, status.Value));
            }

            if (category.HasValue)
            {
                filters.Add(filterBuilder.Eq(c => c.Category, category.Value));
            }

            if (!string.IsNullOrEmpty(organizationId))
            {
                filters.Add(filterBuilder.Eq(c => c.OrganizationId, organizationId));
            }

            if (isFeatured.HasValue)
            {
                filters.Add(filterBuilder.Eq(c => c.IsFeatured, isFeatured.Value));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Create sort definition
            SortDefinition<Campaign> sortDefinition;

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Map sortBy string to property expression
                switch (sortBy.ToLower())
                {
                    case "goal":
                        sortDefinition = ascending
                            ? Builders<Campaign>.Sort.Ascending(c => c.Goal)
                            : Builders<Campaign>.Sort.Descending(c => c.Goal);
                        break;
                    case "amountraised":
                        sortDefinition = ascending
                            ? Builders<Campaign>.Sort.Ascending(c => c.AmountRaised)
                            : Builders<Campaign>.Sort.Descending(c => c.AmountRaised);
                        break;
                    case "progress":
                        sortDefinition = ascending
                            ? Builders<Campaign>.Sort.Ascending(c => c.AmountRaised / c.Goal)
                            : Builders<Campaign>.Sort.Descending(c => c.AmountRaised / c.Goal);
                        break;
                    case "startdate":
                        sortDefinition = ascending
                            ? Builders<Campaign>.Sort.Ascending(c => c.StartDate)
                            : Builders<Campaign>.Sort.Descending(c => c.StartDate);
                        break;
                    case "enddate":
                        sortDefinition = ascending
                            ? Builders<Campaign>.Sort.Ascending(c => c.EndDate)
                            : Builders<Campaign>.Sort.Descending(c => c.EndDate);
                        break;
                    case "views":
                        sortDefinition = ascending
                            ? Builders<Campaign>.Sort.Ascending(c => c.ViewCount)
                            : Builders<Campaign>.Sort.Descending(c => c.ViewCount);
                        break;
                    default:
                        sortDefinition = Builders<Campaign>.Sort.Descending(c => c.CreatedAt);
                        break;
                }
            }
            else
            {
                // Default sort by created date descending (newest first)
                sortDefinition = Builders<Campaign>.Sort.Descending(c => c.CreatedAt);
            }

            return await _collection
                .Find(combinedFilter)
                .Sort(sortDefinition)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountCampaignsAsync(
            CampaignStatus? status = null,
            CampaignCategory? category = null,
            string? organizationId = null,
            bool? isFeatured = null)
        {
            var filterBuilder = Builders<Campaign>.Filter;
            var filters = new List<FilterDefinition<Campaign>>();

            // Apply filters
            if (status.HasValue)
            {
                filters.Add(filterBuilder.Eq(c => c.Status, status.Value));
            }

            if (category.HasValue)
            {
                filters.Add(filterBuilder.Eq(c => c.Category, category.Value));
            }

            if (!string.IsNullOrEmpty(organizationId))
            {
                filters.Add(filterBuilder.Eq(c => c.OrganizationId, organizationId));
            }

            if (isFeatured.HasValue)
            {
                filters.Add(filterBuilder.Eq(c => c.IsFeatured, isFeatured.Value));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return await _collection.CountDocumentsAsync(combinedFilter);
        }

        public async Task<Dictionary<string, object>> GetCampaignStatisticsAsync()
        {
            try
            {
                // Use aggregation pipeline for stats
                var pipeline = new[]
                {
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", null },
                        { "totalCampaigns", new BsonDocument("$sum", 1) },
                        { "totalGoalAmount", new BsonDocument("$sum", "$Goal") },
                        { "totalRaisedAmount", new BsonDocument("$sum", "$AmountRaised") },
                        { "avgGoalAmount", new BsonDocument("$avg", "$Goal") },
                        { "avgRaisedAmount", new BsonDocument("$avg", "$AmountRaised") },
                        { "maxRaisedAmount", new BsonDocument("$max", "$AmountRaised") },
                        { "minGoalAmount", new BsonDocument("$min", "$Goal") },
                        { "maxGoalAmount", new BsonDocument("$max", "$Goal") }
                    })
                };

                var results = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                if (results != null)
                {
                    // Status distribution using another aggregation
                    var statusPipeline = new[]
                    {
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", "$Status" },
                            { "count", new BsonDocument("$sum", 1) }
                        })
                    };

                    var statusResults = await _collection.Aggregate<BsonDocument>(statusPipeline).ToListAsync();
                    var statusDistribution = statusResults.ToDictionary(
                        x => x["_id"].AsString,
                        x => x["count"].AsInt32
                    );

                    // Category distribution
                    var categoryPipeline = new[]
                    {
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", "$Category" },
                            { "count", new BsonDocument("$sum", 1) }
                        })
                    };

                    var categoryResults = await _collection.Aggregate<BsonDocument>(categoryPipeline).ToListAsync();
                    var categoryDistribution = categoryResults.ToDictionary(
                        x => x["_id"].AsString,
                        x => x["count"].AsInt32
                    );

                    // Combine all results
                    var stats = new Dictionary<string, object>
                    {
                        { "totalCampaigns", results["totalCampaigns"].AsInt32 },
                        { "totalGoalAmount", results["totalGoalAmount"].AsDecimal },
                        { "totalRaisedAmount", results["totalRaisedAmount"].AsDecimal },
                        { "avgGoalAmount", results["avgGoalAmount"].AsDecimal },
                        { "avgRaisedAmount", results["avgRaisedAmount"].AsDecimal },
                        { "maxRaisedAmount", results["maxRaisedAmount"].AsDecimal },
                        { "minGoalAmount", results["minGoalAmount"].AsDecimal },
                        { "maxGoalAmount", results["maxGoalAmount"].AsDecimal },
                        { "overallSuccessRate", results["totalRaisedAmount"].AsDecimal / results["totalGoalAmount"].AsDecimal * 100 },
                        { "statusDistribution", statusDistribution },
                        { "categoryDistribution", categoryDistribution }
                    };

                    return stats;
                }

                return new Dictionary<string, object>
                {
                    { "totalCampaigns", 0 },
                    { "totalGoalAmount", 0m },
                    { "totalRaisedAmount", 0m }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign statistics");
                return new Dictionary<string, object>
                {
                    { "error", "Failed to retrieve campaign statistics" }
                };
            }
        }

        /// <summary>
        /// Get campaigns with advanced filtering and pagination
        /// </summary>
        /// <param name="filterParams">Campaign-specific filter parameters</param>
        /// <returns>Paginated and filtered campaign results</returns>
        public async Task<PagedResult<Campaign>> GetCampaignsAsync(CampaignFilterParameters filterParams)
        {
            try
            {
                // Build filter expression from parameters
                var filterExpression = BuildFilterExpression(filterParams);
                
                // Call the base GetPagedAsync with our filter
                return await GetPagedAsync(filterParams, filterExpression);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaigns with filtering: {Message}", ex.Message);
                return PagedResult<Campaign>.Empty(filterParams.PageNumber, filterParams.PageSize);
            }
        }

        /// <summary>
        /// Build a MongoDB filter expression from the campaign filter parameters
        /// </summary>
        /// <param name="filterParams">Filter parameters</param>
        /// <returns>MongoDB filter expression</returns>
        private Expression<Func<Campaign, bool>> BuildFilterExpression(CampaignFilterParameters filterParams)
        {
            Expression<Func<Campaign, bool>> filter = campaign => true; // Start with "select all"
            
            // Apply each filter parameter if provided
            if (filterParams.Category.HasValue)
            {
                var category = filterParams.Category.Value;
                filter = filter.And(c => c.Category == category);
            }
            
            if (filterParams.Status.HasValue)
            {
                var status = filterParams.Status.Value;
                filter = filter.And(c => c.Status == status);
            }
            
            if (!string.IsNullOrWhiteSpace(filterParams.OrganizationId))
            {
                var orgId = filterParams.OrganizationId;
                filter = filter.And(c => c.OrganizationId == orgId);
            }
            
            if (filterParams.IsFeatured.HasValue)
            {
                var isFeatured = filterParams.IsFeatured.Value;
                filter = filter.And(c => c.IsFeatured == isFeatured);
            }
            
            if (filterParams.MinGoal.HasValue)
            {
                var minGoal = filterParams.MinGoal.Value;
                filter = filter.And(c => c.Goal >= minGoal);
            }
            
            if (filterParams.MaxGoal.HasValue)
            {
                var maxGoal = filterParams.MaxGoal.Value;
                filter = filter.And(c => c.Goal <= maxGoal);
            }

            // Progress percentage filtering requires a more complex expression
            if (filterParams.MinProgressPercentage.HasValue)
            {
                var minProgress = filterParams.MinProgressPercentage.Value / 100m; // Convert percentage to decimal
                filter = filter.And(c => (c.AmountRaised / c.Goal) >= minProgress);
            }
            
            if (filterParams.MaxProgressPercentage.HasValue)
            {
                var maxProgress = filterParams.MaxProgressPercentage.Value / 100m; // Convert percentage to decimal
                filter = filter.And(c => (c.AmountRaised / c.Goal) <= maxProgress);
            }
            
            if (filterParams.StartDateFrom.HasValue)
            {
                var startFrom = filterParams.StartDateFrom.Value;
                filter = filter.And(c => c.StartDate >= startFrom);
            }
            
            if (filterParams.StartDateTo.HasValue)
            {
                var startTo = filterParams.StartDateTo.Value;
                filter = filter.And(c => c.StartDate <= startTo);
            }
            
            if (filterParams.EndDateFrom.HasValue)
            {
                var endFrom = filterParams.EndDateFrom.Value;
                filter = filter.And(c => c.EndDate >= endFrom);
            }
            
            if (filterParams.EndDateTo.HasValue)
            {
                var endTo = filterParams.EndDateTo.Value;
                filter = filter.And(c => c.EndDate <= endTo);
            }
            
            if (filterParams.IsActive.HasValue && filterParams.IsActive.Value)
            {
                var now = DateTime.UtcNow;
                filter = filter.And(c => c.Status == CampaignStatus.Active &&
                                      c.StartDate <= now &&
                                      c.EndDate >= now);
            }
            
            if (!string.IsNullOrWhiteSpace(filterParams.SearchText))
            {
                var searchText = filterParams.SearchText.ToLower();
                filter = filter.And(c => c.Title.ToLower().Contains(searchText) || 
                                      c.Description.ToLower().Contains(searchText));
            }
            
            // Location-based filtering will be handled separately because it's more complex
            // and requires geospatial queries that don't translate well to LINQ expressions
            
            if (filterParams.Tags != null && filterParams.Tags.Any())
            {
                filter = filter.And(c => c.Tags != null && filterParams.Tags.All(tag => c.Tags.Contains(tag)));
            }
            
            return filter;
        }
        
        /// <summary>
        /// Override the base GetSortDefinition to add campaign-specific sorting
        /// </summary>
        protected override SortDefinition<Campaign>? GetSortDefinition(string sortBy, bool ascending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                // Implement campaign-specific sort fields
                "goal" => ascending
                    ? Builders<Campaign>.Sort.Ascending(c => c.Goal)
                    : Builders<Campaign>.Sort.Descending(c => c.Goal),
                "amountraised" => ascending
                    ? Builders<Campaign>.Sort.Ascending(c => c.AmountRaised)
                    : Builders<Campaign>.Sort.Descending(c => c.AmountRaised),
                "progress" => ascending
                    ? Builders<Campaign>.Sort.Ascending(c => c.AmountRaised / c.Goal)
                    : Builders<Campaign>.Sort.Descending(c => c.AmountRaised / c.Goal),
                "startdate" => ascending
                    ? Builders<Campaign>.Sort.Ascending(c => c.StartDate)
                    : Builders<Campaign>.Sort.Descending(c => c.StartDate),
                "enddate" => ascending
                    ? Builders<Campaign>.Sort.Ascending(c => c.EndDate)
                    : Builders<Campaign>.Sort.Descending(c => c.EndDate),
                "views" => ascending
                    ? Builders<Campaign>.Sort.Ascending(c => c.ViewCount)
                    : Builders<Campaign>.Sort.Descending(c => c.ViewCount),
                "title" => ascending
                    ? Builders<Campaign>.Sort.Ascending(c => c.Title)
                    : Builders<Campaign>.Sort.Descending(c => c.Title),
                // Fall back to base implementation for common fields
                _ => base.GetSortDefinition(sortBy, ascending)
            };
        }
        
        // Helper extension method for combining expressions
        private static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(T));
            
            var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
            var leftExpression = leftVisitor.Visit(left.Body);
            
            var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
            var rightExpression = rightVisitor.Visit(right.Body);
            
            var andExpression = Expression.AndAlso(leftExpression, rightExpression);
            
            return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
        }
        
        private class ReplaceExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;
            
            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }
            
            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                    
                return base.Visit(node);
            }
        }

        /// <summary>
        /// Get campaigns near a specific location with pagination
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="maxDistanceKm">Maximum distance in kilometers</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated campaigns sorted by distance</returns>
        public async Task<PagedResult<Campaign>> GetNearbyAsync(
            double latitude, 
            double longitude, 
            double maxDistanceKm = 50,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                // Create a geospatial query
                var point = new BsonDocument
                {
                    { "type", "Point" },
                    { "coordinates", new BsonArray { longitude, latitude } }
                };
                
                var filter = Builders<Campaign>.Filter.NearSphere(
                    c => c.Location.Coordinates, 
                    longitude, 
                    latitude, 
                    maxDistanceKm * 1000 // convert to meters
                );
                
                // Get count for pagination
                var totalCount = await _collection.CountDocumentsAsync(filter);
                
                if (totalCount == 0)
                {
                    return PagedResult<Campaign>.Empty(pageNumber, pageSize);
                }
                
                // Execute the query with pagination
                var skip = (pageNumber - 1) * pageSize;
                var campaigns = await _collection
                    .Find(filter)
                    .Skip(skip)
                    .Limit(pageSize)
                    .ToListAsync();
                
                return PagedResult<Campaign>.Create(
                    campaigns,
                    totalCount,
                    pageNumber,
                    pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nearby campaigns: {Message}", ex.Message);
                return PagedResult<Campaign>.Empty(pageNumber, pageSize);
            }
        }
    }
}