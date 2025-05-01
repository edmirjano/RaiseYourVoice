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
            // Target users with this user ID in their target audience
            var filter = Builders<Notification>.Filter.Where(n => 
                (n.TargetAudience.Type == TargetType.AllUsers) ||
                (n.TargetAudience.Type == TargetType.SpecificUsers && 
                 n.TargetAudience.UserIds != null && 
                 n.TargetAudience.UserIds.Contains(userId)));
            
            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId, int limit = 20)
        {
            // Find notifications targeted at this user that are not marked as read
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Where(n => 
                    (n.TargetAudience.Type == TargetType.AllUsers) ||
                    (n.TargetAudience.Type == TargetType.SpecificUsers && 
                     n.TargetAudience.UserIds != null && 
                     n.TargetAudience.UserIds.Contains(userId))),
                Builders<Notification>.Filter.Ne(n => n.ReadStatus, ReadStatus.Read)
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
            var update = Builders<Notification>.Update
                .Set(n => n.ReadStatus, ReadStatus.Read)
                .Set(n => n.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            // Find all notifications targeting this user
            var filter = Builders<Notification>.Filter.Where(n => 
                (n.TargetAudience.Type == TargetType.AllUsers) ||
                (n.TargetAudience.Type == TargetType.SpecificUsers && 
                 n.TargetAudience.UserIds != null && 
                 n.TargetAudience.UserIds.Contains(userId)));
            
            var update = Builders<Notification>.Update
                .Set(n => n.ReadStatus, ReadStatus.Read)
                .Set(n => n.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateManyAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            // Find unread notifications for this user
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Where(n => 
                    (n.TargetAudience.Type == TargetType.AllUsers) ||
                    (n.TargetAudience.Type == TargetType.SpecificUsers && 
                     n.TargetAudience.UserIds != null && 
                     n.TargetAudience.UserIds.Contains(userId))),
                Builders<Notification>.Filter.Ne(n => n.ReadStatus, ReadStatus.Read)
            );
            
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<IEnumerable<Notification>> GetExpiredNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Ne(n => n.ExpiresAt, null),
                Builders<Notification>.Filter.Lt(n => n.ExpiresAt, now)
            );
            
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> DeleteExpiredNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Ne(n => n.ExpiresAt, null),
                Builders<Notification>.Filter.Lt(n => n.ExpiresAt, now)
            );
            
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> AddRecipientAsync(string notificationId, string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            
            // Check if the notification has SpecificUsers target type
            var notification = await _collection.Find(filter).FirstOrDefaultAsync();
            if (notification == null || notification.TargetAudience.Type != TargetType.SpecificUsers)
            {
                return false;
            }
            
            // Initialize UserIds array if null
            var userIds = notification.TargetAudience.UserIds?.ToList() ?? new List<string>();
            
            // Add the user if not already in the list
            if (!userIds.Contains(userId))
            {
                userIds.Add(userId);
                
                var update = Builders<Notification>.Update
                    .Set(n => n.TargetAudience.UserIds, userIds.ToArray())
                    .Set(n => n.UpdatedAt, DateTime.UtcNow);
                
                var result = await _collection.UpdateOneAsync(filter, update);
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            
            return true; // User was already a recipient
        }
    }
}