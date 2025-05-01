using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class EncryptionKeyRepository : MongoRepository<EncryptionKey>, IEncryptionKeyRepository
    {
        protected readonly IMongoCollection<EncryptionKey> _collection;
        
        public EncryptionKeyRepository(MongoDbContext context) 
            : base(context, "EncryptionKeys")
        {
            _collection = context.Database.GetCollection<EncryptionKey>("EncryptionKeys");
        }

        public async Task<EncryptionKey> GetActiveKeyAsync(string purpose)
        {
            return await _collection.Find(k => k.Purpose == purpose && k.IsActive).FirstOrDefaultAsync();
        }

        public async Task<EncryptionKey> GetKeyByVersionAsync(int version, string purpose)
        {
            return await _collection.Find(k => k.Purpose == purpose && k.Version == version).FirstOrDefaultAsync();
        }

        public async Task<List<EncryptionKey>> GetKeysByPurposeAsync(string purpose, bool includeExpired = false)
        {
            var filter = Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose);
            
            if (!includeExpired)
            {
                var now = DateTime.UtcNow;
                filter = Builders<EncryptionKey>.Filter.And(
                    filter,
                    Builders<EncryptionKey>.Filter.Or(
                        Builders<EncryptionKey>.Filter.Eq(k => k.ExpiresAt, null),
                        Builders<EncryptionKey>.Filter.Gt(k => k.ExpiresAt, now)
                    )
                );
            }

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<int> GetHighestVersionAsync(string purpose)
        {
            var key = await _collection.Find(k => k.Purpose == purpose)
                .SortByDescending(k => k.Version)
                .FirstOrDefaultAsync();
                
            return key?.Version ?? 0;
        }

        public async Task<bool> DeactivateAllKeysByPurposeAsync(string purpose)
        {
            var update = Builders<EncryptionKey>.Update
                .Set(k => k.IsActive, false)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateManyAsync(k => k.Purpose == purpose && k.IsActive, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> ActivateKeyAsync(string keyId, string purpose)
        {
            // First deactivate all keys for this purpose
            await DeactivateAllKeysByPurposeAsync(purpose);

            // Then activate the specific key
            var update = Builders<EncryptionKey>.Update
                .Set(k => k.IsActive, true)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(k => k.Id == keyId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}