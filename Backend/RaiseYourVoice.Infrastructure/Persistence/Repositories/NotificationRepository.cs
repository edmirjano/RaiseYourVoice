using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : MongoGenericRepository<Notification>
    {
        public NotificationRepository(MongoDbContext context) : base(context, "Notifications")
        {
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, int limit = 20, int skip = 0)
        {
            var filter = Builders<Notification>.Filter.AnyEq(n => n.RecipientIds, userId);
            
            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId, int limit = 20)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.AnyEq(n => n.RecipientIds, userId),
                Builders<Notification>.Filter.AnyNe(n => n.ReadByUserIds, userId)
            );
            
            return await _collection.Find(filter)
                .Limit(limit)
                .SortByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type, int limit = 20, int skip = 0)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Type, type);
            
            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<Notification>.Update.AddToSet(n => n.ReadByUserIds, userId);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.AnyEq(n => n.RecipientIds, userId),
                Builders<Notification>.Filter.AnyNe(n => n.ReadByUserIds, userId)
            );
            
            var update = Builders<Notification>.Update.AddToSet(n => n.ReadByUserIds, userId);
            
            var result = await _collection.UpdateManyAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.AnyEq(n => n.RecipientIds, userId),
                Builders<Notification>.Filter.AnyNe(n => n.ReadByUserIds, userId)
            );
            
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<IEnumerable<Notification>> GetExpiredNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var filter = Builders<Notification>.Filter.Lt(n => n.ExpiresAt, now);
            
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> DeleteExpiredNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var filter = Builders<Notification>.Filter.Lt(n => n.ExpiresAt, now);
            
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> AddRecipientAsync(string notificationId, string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<Notification>.Update.AddToSet(n => n.RecipientIds, userId);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}