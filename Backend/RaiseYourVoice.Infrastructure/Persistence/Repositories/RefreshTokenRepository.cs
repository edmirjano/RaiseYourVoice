using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly IMongoCollection<RefreshToken> _refreshTokens;

        public RefreshTokenRepository(IMongoClient mongoClient) : base(mongoClient, "RefreshTokens")
        {
            var database = mongoClient.GetDatabase("RaiseYourVoice");
            _refreshTokens = database.GetCollection<RefreshToken>("RefreshTokens");
        }

        public async Task<RefreshToken?> FindValidTokenAsync(string userId, string token)
        {
            return await _refreshTokens
                .Find(rt => rt.UserId == userId && rt.Token == token && rt.ExpiryDate > DateTime.UtcNow && !rt.IsRevoked)
                .FirstOrDefaultAsync();
        }

        public async Task MarkTokenAsRevokedAsync(string userId, string token)
        {
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow)
                .Set(rt => rt.UpdatedAt, DateTime.UtcNow);
            
            await _refreshTokens.UpdateOneAsync(
                rt => rt.UserId == userId && rt.Token == token,
                update);
        }
    }
}