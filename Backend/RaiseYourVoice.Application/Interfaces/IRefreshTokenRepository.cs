using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Application.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> FindValidTokenAsync(string userId, string token);
        Task MarkTokenAsRevokedAsync(string userId, string token);
    }
}