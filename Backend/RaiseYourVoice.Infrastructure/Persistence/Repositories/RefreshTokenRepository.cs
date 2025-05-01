using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Infrastructure.Persistence;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : MongoGenericRepository<RefreshToken>
    {
        public RefreshTokenRepository(MongoDbContext context) : base(context, "RefreshTokens")
        {
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> RevokeAsync(string token)
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);
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
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            var filter = Builders<RefreshToken>.Filter.Lt(rt => rt.ExpiresAt, now);
            
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            var now = DateTime.UtcNow;
            var filter = Builders<RefreshToken>.Filter.And(
                Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token),
                Builders<RefreshToken>.Filter.Eq(rt => rt.IsRevoked, false),
                Builders<RefreshToken>.Filter.Gt(rt => rt.ExpiresAt, now)
            );
            
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<bool> UseTokenAsync(string token, string newToken, DateTime newTokenExpiryDate)
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow)
                .Set(rt => rt.ReplacedByToken, newToken)
                .Set(rt => rt.UpdatedAt, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}