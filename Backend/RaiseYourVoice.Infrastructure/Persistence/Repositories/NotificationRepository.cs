using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : MongoRepository<Notification>
    {
        public NotificationRepository(MongoDbContext context, ILogger<NotificationRepository> logger) 
            : base(context, "Notifications", logger)
        {
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.AnyEq("TargetAudience.UserIds", userId);
            return await _collection.Find(filter)
                .Sort(Builders<Notification>.Sort.Descending(n => n.CreatedAt))
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.AnyEq("TargetAudience.UserIds", userId),
                Builders<Notification>.Filter.Not(
                    Builders<Notification>.Filter.AnyEq("ReadByUserIds", userId)
                )
            );

            return await _collection.Find(filter)
                .Sort(Builders<Notification>.Sort.Descending(n => n.CreatedAt))
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByRoleAsync(UserRole role)
        {
            var filter = Builders<Notification>.Filter.AnyEq("TargetAudience.Roles", role);
            return await _collection.Find(filter)
                .Sort(Builders<Notification>.Sort.Descending(n => n.CreatedAt))
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetActiveNotificationsAsync()
        {
            var currentTime = DateTime.UtcNow;
            var filter = Builders<Notification>.Filter.Or(
                Builders<Notification>.Filter.Gt(n => n.ExpiresAt, currentTime),
                Builders<Notification>.Filter.Eq(n => n.ExpiresAt, null)
            );

            return await _collection.Find(filter)
                .Sort(Builders<Notification>.Sort.Descending(n => n.CreatedAt))
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<Notification>.Update.AddToSet("ReadByUserIds", userId);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.AnyEq("TargetAudience.UserIds", userId),
                Builders<Notification>.Filter.Not(
                    Builders<Notification>.Filter.AnyEq("ReadByUserIds", userId)
                )
            );

            var update = Builders<Notification>.Update.AddToSet("ReadByUserIds", userId);
            var result = await _collection.UpdateManyAsync(filter, update);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Notification>> GetPaginatedNotificationsAsync(
            string userId,
            int pageNumber,
            int pageSize,
            bool unreadOnly = false,
            NotificationType? type = null)
        {
            var filterBuilder = Builders<Notification>.Filter;
            var filters = new List<FilterDefinition<Notification>>();

            // Base filter: notifications targeted to this user
            filters.Add(filterBuilder.AnyEq("TargetAudience.UserIds", userId));

            // Add filter for unread notifications
            if (unreadOnly)
            {
                filters.Add(filterBuilder.Not(
                    filterBuilder.AnyEq("ReadByUserIds", userId)
                ));
            }

            // Add filter for notification type
            if (type.HasValue)
            {
                filters.Add(filterBuilder.Eq(n => n.Type, type.Value));
            }

            var combinedFilter = filterBuilder.And(filters);

            return await _collection
                .Find(combinedFilter)
                .Sort(Builders<Notification>.Sort.Descending(n => n.CreatedAt))
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountNotificationsAsync(
            string userId,
            bool unreadOnly = false,
            NotificationType? type = null)
        {
            var filterBuilder = Builders<Notification>.Filter;
            var filters = new List<FilterDefinition<Notification>>();

            // Base filter: notifications targeted to this user
            filters.Add(filterBuilder.AnyEq("TargetAudience.UserIds", userId));

            // Add filter for unread notifications
            if (unreadOnly)
            {
                filters.Add(filterBuilder.Not(
                    filterBuilder.AnyEq("ReadByUserIds", userId)
                ));
            }

            // Add filter for notification type
            if (type.HasValue)
            {
                filters.Add(filterBuilder.Eq(n => n.Type, type.Value));
            }

            var combinedFilter = filterBuilder.And(filters);
            return await _collection.CountDocumentsAsync(combinedFilter);
        }

        public async Task<bool> DeleteExpiredNotificationsAsync()
        {
            var currentTime = DateTime.UtcNow;
            var filter = Builders<Notification>.Filter.Lt(n => n.ExpiresAt, currentTime);

            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> AddDeliveryStatusAsync(string notificationId, string userId, DeliveryStatus status, string? deviceId = null)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            
            // Create delivery status
            var deliveryStatus = new NotificationDeliveryStatus
            {
                UserId = userId,
                Status = status,
                Timestamp = DateTime.UtcNow,
                DeviceId = deviceId
            };

            var update = Builders<Notification>.Update.Push(n => n.DeliveryStatuses, deliveryStatus);
            var result = await _collection.UpdateOneAsync(filter, update);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<Dictionary<string, int>> GetNotificationStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var filterBuilder = Builders<Notification>.Filter;
            var filters = new List<FilterDefinition<Notification>>();

            if (startDate.HasValue)
            {
                filters.Add(filterBuilder.Gte(n => n.CreatedAt, startDate.Value));
            }

            if (endDate.HasValue)
            {
                filters.Add(filterBuilder.Lte(n => n.CreatedAt, endDate.Value));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Get basic counts
            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            // Type breakdown using aggregation
            var typePipeline = new[]
            {
                new BsonDocument("$match", combinedFilter.Render(
                    _collection.DocumentSerializer, 
                    _collection.Settings.SerializerRegistry)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$Type" },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

            var typeResults = await _collection.Aggregate<BsonDocument>(typePipeline).ToListAsync();
            var typeStats = typeResults.ToDictionary(
                r => r["_id"].AsString,
                r => r["count"].AsInt32
            );

            // Combine all stats
            var statistics = new Dictionary<string, int>
            {
                { "totalCount", (int)totalCount }
            };

            // Add type counts
            foreach (var typeStat in typeStats)
            {
                statistics[$"type_{typeStat.Key}"] = typeStat.Value;
            }

            return statistics;
        }
    }
}