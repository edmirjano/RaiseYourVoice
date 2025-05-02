using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class UserRepository : MongoRepository<User>
    {
        public UserRepository(MongoDbContext context, ILogger<UserRepository> logger) 
            : base(context, "Users", logger)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, role);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> UpdateLastLoginAsync(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.LastLogin, DateTime.UtcNow)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateProfileAsync(string userId, string name, string? bio, string? profilePicture)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var updateBuilder = Builders<User>.Update
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(name))
            {
                updateBuilder = updateBuilder.Set(u => u.Name, name);
            }
            
            if (bio != null) // can be empty string to clear bio
            {
                updateBuilder = updateBuilder.Set(u => u.Bio, bio);
            }
            
            if (profilePicture != null) // can be empty string to clear profile picture
            {
                updateBuilder = updateBuilder.Set(u => u.ProfilePicture, profilePicture);
            }

            var result = await _collection.UpdateOneAsync(filter, updateBuilder);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdatePasswordAsync(string userId, string passwordHash)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, passwordHash)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateRoleAsync(string userId, UserRole role)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.Role, role)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.Preferences, preferences)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateNotificationSettingsAsync(string userId, NotificationSettings settings)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.Preferences.NotificationSettings, settings)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> AddDeviceTokenAsync(string userId, string deviceToken, string deviceId, string? platform = null)
        {
            var device = new UserDevice
            {
                DeviceId = deviceId,
                DeviceToken = deviceToken,
                Platform = platform,
                LastUpdated = DateTime.UtcNow
            };

            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.Not(Builders<User>.Filter.ElemMatch(
                    u => u.DeviceTokens, d => d.DeviceId == deviceId)
                )
            );

            var update = Builders<User>.Update
                .Push(u => u.DeviceTokens, device)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            
            // If device already exists, we need to update it
            if (result.MatchedCount == 0)
            {
                filter = Builders<User>.Filter.And(
                    Builders<User>.Filter.Eq(u => u.Id, userId),
                    Builders<User>.Filter.ElemMatch(u => u.DeviceTokens, d => d.DeviceId == deviceId)
                );
                
                update = Builders<User>.Update
                    .Set(u => u.DeviceTokens[-1].DeviceToken, deviceToken)
                    .Set(u => u.DeviceTokens[-1].LastUpdated, DateTime.UtcNow)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);
                
                if (!string.IsNullOrEmpty(platform))
                {
                    update = update.Set(u => u.DeviceTokens[-1].Platform, platform);
                }
                
                result = await _collection.UpdateOneAsync(filter, update);
            }
            
            return result.IsAcknowledged && (result.MatchedCount > 0);
        }

        public async Task<bool> RemoveDeviceTokenAsync(string userId, string deviceId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .PullFilter(u => u.DeviceTokens, d => d.DeviceId == deviceId)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<User>> GetPaginatedUsersAsync(
            int pageNumber, 
            int pageSize,
            UserRole? role = null,
            DateTime? joinedAfter = null,
            DateTime? joinedBefore = null,
            string? sortBy = null,
            bool ascending = true)
        {
            var filterBuilder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();

            // Apply filters
            if (role.HasValue)
            {
                filters.Add(filterBuilder.Eq(u => u.Role, role.Value));
            }

            if (joinedAfter.HasValue)
            {
                filters.Add(filterBuilder.Gte(u => u.JoinDate, joinedAfter.Value));
            }

            if (joinedBefore.HasValue)
            {
                filters.Add(filterBuilder.Lte(u => u.JoinDate, joinedBefore.Value));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Create sort definition
            SortDefinition<User> sortDefinition;

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Map sortBy string to property expression
                switch (sortBy.ToLower())
                {
                    case "name":
                        sortDefinition = ascending
                            ? Builders<User>.Sort.Ascending(u => u.Name)
                            : Builders<User>.Sort.Descending(u => u.Name);
                        break;
                    case "joindate":
                        sortDefinition = ascending
                            ? Builders<User>.Sort.Ascending(u => u.JoinDate)
                            : Builders<User>.Sort.Descending(u => u.JoinDate);
                        break;
                    case "lastlogin":
                        sortDefinition = ascending
                            ? Builders<User>.Sort.Ascending(u => u.LastLogin)
                            : Builders<User>.Sort.Descending(u => u.LastLogin);
                        break;
                    default:
                        sortDefinition = Builders<User>.Sort.Ascending(u => u.Name);
                        break;
                }
            }
            else
            {
                // Default sort by name ascending
                sortDefinition = Builders<User>.Sort.Ascending(u => u.Name);
            }

            return await _collection
                .Find(combinedFilter)
                .Sort(sortDefinition)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountUsersAsync(
            UserRole? role = null,
            DateTime? joinedAfter = null,
            DateTime? joinedBefore = null)
        {
            var filterBuilder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();

            // Apply filters
            if (role.HasValue)
            {
                filters.Add(filterBuilder.Eq(u => u.Role, role.Value));
            }

            if (joinedAfter.HasValue)
            {
                filters.Add(filterBuilder.Gte(u => u.JoinDate, joinedAfter.Value));
            }

            if (joinedBefore.HasValue)
            {
                filters.Add(filterBuilder.Lte(u => u.JoinDate, joinedBefore.Value));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return await _collection.CountDocumentsAsync(combinedFilter);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _collection.CountDocumentsAsync(filter) > 0;
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<User>.Filter.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            return await _collection.Find(filter).ToListAsync();
        }
    }
}