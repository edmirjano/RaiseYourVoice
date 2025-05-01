using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class EncryptionKeyRepository : MongoGenericRepository<EncryptionKey>, IEncryptionKeyRepository
    {
        public EncryptionKeyRepository(MongoDbContext context) 
            : base(context, "EncryptionKeys")
        {
        }

        public async Task<EncryptionKey> GetActiveKeyAsync(string purpose)
        {
            return await _collection.Find(k => k.Purpose == purpose && k.IsActive).FirstOrDefaultAsync();
        }

        public async Task<EncryptionKey> GetActiveKeyByPurposeAsync(string purpose)
        {
            return await _collection.Find(k => k.Purpose == purpose && k.IsActive).FirstOrDefaultAsync();
        }

        public async Task<List<EncryptionKey>> GetKeysByPurposeAsync(string purpose, bool activeOnly = false)
        {
            var filter = activeOnly 
                ? Builders<EncryptionKey>.Filter.And(
                    Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose),
                    Builders<EncryptionKey>.Filter.Eq(k => k.IsActive, true))
                : Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose);

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<EncryptionKey>> GetKeysByPurposeAsync(string purpose)
        {
            return await _collection.Find(k => k.Purpose == purpose).ToListAsync();
        }

        public async Task<IEnumerable<EncryptionKey>> GetExpiredKeysAsync()
        {
            var now = DateTime.UtcNow;
            return await _collection.Find(k => k.ExpiresAt != null && k.ExpiresAt < now).ToListAsync();
        }

        public async Task<EncryptionKey> GetKeyByVersionAsync(int version, string purpose)
        {
            return await _collection.Find(k => k.Purpose == purpose && k.Version == version).FirstOrDefaultAsync();
        }

        public async Task<EncryptionKey> GetKeyByVersionAsync(string purpose, int version)
        {
            return await _collection.Find(k => k.Purpose == purpose && k.Version == version).FirstOrDefaultAsync();
        }

        public async Task<int> GetLatestVersionByPurposeAsync(string purpose)
        {
            var key = await _collection.Find(k => k.Purpose == purpose)
                .SortByDescending(k => k.Version)
                .FirstOrDefaultAsync();
                
            return key?.Version ?? 0;
        }

        public async Task<int> GetHighestVersionAsync(string purpose)
        {
            var key = await _collection.Find(k => k.Purpose == purpose)
                .SortByDescending(k => k.Version)
                .FirstOrDefaultAsync();
                
            return key?.Version ?? 0;
        }

        public async Task<bool> DeactivateKeyAsync(string id)
        {
            var update = Builders<EncryptionKey>.Update
                .Set(k => k.IsActive, false)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(k => k.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeactivateAllKeysByPurposeAsync(string purpose)
        {
            var update = Builders<EncryptionKey>.Update
                .Set(k => k.IsActive, false)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateManyAsync(k => k.Purpose == purpose && k.IsActive, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> ActivateKeyAsync(string id, string purpose)
        {
            // First deactivate all keys for this purpose
            await DeactivateAllKeysByPurposeAsync(purpose);

            // Then activate the specific key
            var update = Builders<EncryptionKey>.Update
                .Set(k => k.IsActive, true)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(k => k.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}