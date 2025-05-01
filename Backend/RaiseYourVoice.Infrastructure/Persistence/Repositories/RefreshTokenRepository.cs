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

        public async Task<RefreshToken?> FindValidTokenAsync(string token, string userId)
        {
            var now = DateTime.UtcNow;
            return await _collection.Find(rt => 
                rt.Token == token && 
                rt.UserId == userId && 
                rt.ExpiryDate > now && 
                !rt.IsRevoked)
                .FirstOrDefaultAsync();
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

        public async Task MarkTokenAsRevokedAsync(string token, string reason)
        {
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow)
                .Set(rt => rt.ReasonRevoked, reason);
                
            await _collection.UpdateOneAsync(rt => rt.Token == token, update);
        }

        public async Task<bool> DeleteExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            var result = await _collection.DeleteManyAsync(rt => rt.ExpiryDate < now);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}