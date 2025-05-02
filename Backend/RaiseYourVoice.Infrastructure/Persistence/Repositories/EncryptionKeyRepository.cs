using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class EncryptionKeyRepository : MongoRepository<EncryptionKey>, IEncryptionKeyRepository
    {
        public EncryptionKeyRepository(MongoDbContext context, ILogger<EncryptionKeyRepository> logger) 
            : base(context, "EncryptionKeys", logger)
        {
        }

        public async Task<EncryptionKey?> GetActiveKeyAsync(string purpose = "field-encryption")
        {
            var filter = Builders<EncryptionKey>.Filter.And(
                Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                Builders<EncryptionKey>.Filter.Eq(k => k.IsActive, true)
            );
            
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<EncryptionKey?> GetKeyByVersionAsync(int version, string purpose = "field-encryption")
        {
            var filter = Builders<EncryptionKey>.Filter.And(
                Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                Builders<EncryptionKey>.Filter.Eq(k => k.Version, version)
            );
            
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EncryptionKey>> GetAllKeysAsync(string purpose = "field-encryption", bool includeExpired = false)
        {
            var filter = Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose);
            
            if (!includeExpired)
            {
                var currentDate = DateTime.UtcNow;
                filter = Builders<EncryptionKey>.Filter.And(
                    filter,
                    Builders<EncryptionKey>.Filter.Or(
                        Builders<EncryptionKey>.Filter.Gt(k => k.ExpiresAt, currentDate),
                        Builders<EncryptionKey>.Filter.Exists(k => k.ExpiresAt, false)
                    )
                );
            }
            
            return await _collection.Find(filter)
                .Sort(Builders<EncryptionKey>.Sort.Descending(k => k.Version))
                .ToListAsync();
        }

        public async Task<bool> DeactivateAllKeysAsync(string purpose = "field-encryption")
        {
            var filter = Builders<EncryptionKey>.Filter.And(
                Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                Builders<EncryptionKey>.Filter.Eq(k => k.IsActive, true)
            );
            
            var update = Builders<EncryptionKey>.Update
                .Set(k => k.IsActive, false)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateManyAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> ActivateKeyAsync(string keyId)
        {
            // First, deactivate all keys with the same purpose
            var key = await _collection.Find(k => k.Id == keyId).FirstOrDefaultAsync();
            if (key == null)
            {
                return false;
            }
            
            await DeactivateAllKeysAsync(key.Purpose);
            
            // Then, activate the specified key
            var filter = Builders<EncryptionKey>.Filter.Eq(k => k.Id, keyId);
            var update = Builders<EncryptionKey>.Update
                .Set(k => k.IsActive, true)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<int> GetHighestVersionAsync(string purpose = "field-encryption")
        {
            var filter = Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose);
            var sort = Builders<EncryptionKey>.Sort.Descending(k => k.Version);
            
            var highestVersionKey = await _collection.Find(filter)
                .Sort(sort)
                .Limit(1)
                .FirstOrDefaultAsync();
                
            return highestVersionKey?.Version ?? 0;
        }

        public async Task<IEnumerable<EncryptionKey>> GetExpiredKeysAsync(string purpose = "field-encryption")
        {
            var currentDate = DateTime.UtcNow;
            var filter = Builders<EncryptionKey>.Filter.And(
                Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                Builders<EncryptionKey>.Filter.Lt(k => k.ExpiresAt, currentDate),
                Builders<EncryptionKey>.Filter.Ne(k => k.IsRetired, true)
            );
            
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> RetireExpiredKeysAsync(string purpose = "field-encryption")
        {
            var currentDate = DateTime.UtcNow;
            var filter = Builders<EncryptionKey>.Filter.And(
                Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                Builders<EncryptionKey>.Filter.Lt(k => k.ExpiresAt, currentDate),
                Builders<EncryptionKey>.Filter.Ne(k => k.IsRetired, true)
            );
            
            var update = Builders<EncryptionKey>.Update
                .Set(k => k.IsRetired, true)
                .Set(k => k.IsActive, false)
                .Set(k => k.RetiredAt, currentDate)
                .Set(k => k.UpdatedAt, currentDate);
                
            var result = await _collection.UpdateManyAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> KeyExistsAsync(string keyId)
        {
            var filter = Builders<EncryptionKey>.Filter.Eq(k => k.Id, keyId);
            return await _collection.CountDocumentsAsync(filter) > 0;
        }

        public async Task<bool> KeyVersionExistsAsync(int version, string purpose = "field-encryption")
        {
            var filter = Builders<EncryptionKey>.Filter.And(
                Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                Builders<EncryptionKey>.Filter.Eq(k => k.Version, version)
            );
            
            return await _collection.CountDocumentsAsync(filter) > 0;
        }

        public async Task<Dictionary<string, object>> GetKeyMetricsAsync(string purpose = "field-encryption")
        {
            var currentDate = DateTime.UtcNow;
            
            // Get counts of keys in different states
            var totalCount = await _collection.CountDocumentsAsync(
                Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose));
                
            var activeCount = await _collection.CountDocumentsAsync(
                Builders<EncryptionKey>.Filter.And(
                    Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                    Builders<EncryptionKey>.Filter.Eq(k => k.IsActive, true)
                ));
                
            var expiredCount = await _collection.CountDocumentsAsync(
                Builders<EncryptionKey>.Filter.And(
                    Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                    Builders<EncryptionKey>.Filter.Lt(k => k.ExpiresAt, currentDate)
                ));
                
            var retiredCount = await _collection.CountDocumentsAsync(
                Builders<EncryptionKey>.Filter.And(
                    Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                    Builders<EncryptionKey>.Filter.Eq(k => k.IsRetired, true)
                ));
                
            // Get the highest version number
            var highestVersion = await GetHighestVersionAsync(purpose);
            
            // Get the active key's expiration date
            var activeKey = await GetActiveKeyAsync(purpose);
            var daysUntilRotation = activeKey?.ExpiresAt.HasValue == true
                ? (activeKey.ExpiresAt.Value - currentDate).TotalDays
                : null;
                
            return new Dictionary<string, object>
            {
                { "totalCount", totalCount },
                { "activeCount", activeCount },
                { "expiredCount", expiredCount },
                { "retiredCount", retiredCount },
                { "highestVersion", highestVersion },
                { "daysUntilRotation", daysUntilRotation ?? 0 }
            };
        }
    }
}