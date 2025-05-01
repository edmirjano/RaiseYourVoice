using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class EncryptionKeyRepository : MongoRepository<EncryptionKey>, IEncryptionKeyRepository
    {
        private readonly IMongoCollection<EncryptionKey> _encryptionKeys;

        public EncryptionKeyRepository(MongoDbContext context) : base(context)
        {
            _encryptionKeys = context.Database.GetCollection<EncryptionKey>("EncryptionKeys");
        }

        public async Task<EncryptionKey> GetActiveKeyAsync(string purpose)
        {
            return await _encryptionKeys.Find(k => 
                k.Purpose == purpose && 
                k.IsActive && 
                k.ActivatedAt <= DateTime.UtcNow && 
                k.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<EncryptionKey> GetKeyByVersionAsync(int version, string purpose)
        {
            return await _encryptionKeys.Find(k => 
                k.Version == version && 
                k.Purpose == purpose)
                .FirstOrDefaultAsync();
        }

        public async Task<List<EncryptionKey>> GetKeysByPurposeAsync(string purpose, bool includeExpired = false)
        {
            var builder = Builders<EncryptionKey>.Filter;
            var filter = builder.Eq(k => k.Purpose, purpose);

            if (!includeExpired)
            {
                filter = filter & builder.Gt(k => k.ExpiresAt, DateTime.UtcNow);
            }

            return await _encryptionKeys.Find(filter)
                .SortByDescending(k => k.Version)
                .ToListAsync();
        }

        public async Task<bool> ActivateKeyAsync(string keyId, string purpose)
        {
            // Start a transaction
            using var session = await _encryptionKeys.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // Deactivate all keys with the same purpose
                var deactivateFilter = Builders<EncryptionKey>.Filter.Eq(k => k.Purpose, purpose);
                var deactivateUpdate = Builders<EncryptionKey>.Update.Set(k => k.IsActive, false);
                
                await _encryptionKeys.UpdateManyAsync(
                    session,
                    deactivateFilter,
                    deactivateUpdate);

                // Activate the specified key
                var activateFilter = Builders<EncryptionKey>.Filter.Eq(k => k.Id, keyId);
                var activateUpdate = Builders<EncryptionKey>.Update
                    .Set(k => k.IsActive, true)
                    .Set(k => k.ActivatedAt, DateTime.UtcNow);
                
                var result = await _encryptionKeys.UpdateOneAsync(
                    session,
                    activateFilter,
                    activateUpdate);

                // Commit the transaction
                await session.CommitTransactionAsync();
                
                return result.ModifiedCount > 0;
            }
            catch
            {
                // Abort the transaction on error
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task<int> GetHighestVersionAsync(string purpose)
        {
            var highestVersionKey = await _encryptionKeys.Find(k => k.Purpose == purpose)
                .SortByDescending(k => k.Version)
                .Limit(1)
                .FirstOrDefaultAsync();

            return highestVersionKey?.Version ?? 0;
        }
    }
}