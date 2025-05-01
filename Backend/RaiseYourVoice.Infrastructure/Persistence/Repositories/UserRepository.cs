using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class UserRepository : MongoGenericRepository<User>
    {
        public UserRepository(MongoDbContext context) : base(context, "Users")
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetByRoleAsync(UserRole role)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, role);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.Preferences.PreferredLanguage, preferences.PreferredLanguage)
                .Set(u => u.Preferences.NotificationSettings, preferences.NotificationSettings)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> AddDeviceTokenAsync(string userId, string deviceToken)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.AddToSet(u => u.DeviceTokens, deviceToken);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveDeviceTokenAsync(string userId, string deviceToken)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Pull(u => u.DeviceTokens, deviceToken);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}