using System;
using System.Security.Claims;
using System.Threading.Tasks;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user);
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
        Task SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expiry);
        Task RevokeRefreshTokenAsync(string userId, string refreshToken);
    }
}