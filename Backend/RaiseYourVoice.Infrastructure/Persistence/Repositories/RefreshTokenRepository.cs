using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : MongoRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(MongoDbContext context, ILogger<RefreshTokenRepository> logger) 
            : base(context, "RefreshTokens", logger)
        {
        }

        public async Task<RefreshToken?> FindValidTokenAsync(string userId, string token)
        {
            var currentTime = DateTime.UtcNow;
            var filter = Builders<RefreshToken>.Filter.And(
                Builders<RefreshToken>.Filter.Eq(rt => rt.UserId, userId),
                Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token),
                Builders<RefreshToken>.Filter.Gt(rt => rt.ExpiresAt, currentTime),
                Builders<RefreshToken>.Filter.Eq(rt => rt.IsRevoked, false)
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> MarkTokenAsRevokedAsync(string userId, string token)
        {
            var filter = Builders<RefreshToken>.Filter.And(
                Builders<RefreshToken>.Filter.Eq(rt => rt.UserId, userId),
                Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token)
            );

            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow)
                .Set(rt => rt.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> RevokeAllUserTokensAsync(string userId)
        {
            var filter = Builders<RefreshToken>.Filter.And(
                Builders<RefreshToken>.Filter.Eq(rt => rt.UserId, userId),
                Builders<RefreshToken>.Filter.Eq(rt => rt.IsRevoked, false)
            );

            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow)
                .Set(rt => rt.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateManyAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> CleanupExpiredTokensAsync()
        {
            var currentTime = DateTime.UtcNow;
            var filter = Builders<RefreshToken>.Filter.Lt(rt => rt.ExpiresAt, currentTime);
            
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<RefreshToken?> FindTokenByValueAsync(string token)
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateTokenExpirationAsync(string tokenId, DateTime newExpiration)
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Id, tokenId);
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.ExpiresAt, newExpiration)
                .Set(rt => rt.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<long> CountValidTokensForUserAsync(string userId)
        {
            var currentTime = DateTime.UtcNow;
            var filter = Builders<RefreshToken>.Filter.And(
                Builders<RefreshToken>.Filter.Eq(rt => rt.UserId, userId),
                Builders<RefreshToken>.Filter.Gt(rt => rt.ExpiresAt, currentTime),
                Builders<RefreshToken>.Filter.Eq(rt => rt.IsRevoked, false)
            );
            
            return await _collection.CountDocumentsAsync(filter);
        }
    }
}