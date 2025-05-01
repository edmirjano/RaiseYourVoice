using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : MongoGenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(MongoDbContext context) 
            : base(context, "RefreshTokens")
        {
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _collection.Find(rt => rt.Token == token).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(rt => rt.UserId == userId).ToListAsync();
        }

        public async Task<bool> RevokeAllUserTokensAsync(string userId)
        {
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateManyAsync(rt => rt.UserId == userId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(rt => rt.Token == token, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            var result = await _collection.DeleteManyAsync(rt => rt.ExpiresAt < now);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}