using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RaiseYourVoice.Infrastructure.Services.Security
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtKeyManager _keyManager;

        public TokenService(
            IConfiguration configuration, 
            IRefreshTokenRepository refreshTokenRepository,
            JwtKeyManager keyManager)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _keyManager = keyManager;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Get the current signing key with its ID from the key manager
            var (signingKey, keyId) = _keyManager.GetCurrentSigningKey();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Id), // For backward compatibility
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"] ?? "60")),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                // Include the key ID in the token header for key identification during validation
                AdditionalHeaderClaims = new Dictionary<string, object>
                {
                    { "kid", keyId }
                }
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];
                
                // Get all signing keys from the key manager to support tokens signed with previous keys
                var signingKeys = _keyManager.GetAllSigningKeys();
                
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // Don't validate lifetime for expired tokens
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    // Use all available keys for validation to support tokens signed with previous keys
                    IssuerSigningKeys = signingKeys
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user)
        {
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            
            // Store refresh token using repository
            await SaveRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));
            
            return (accessToken, refreshToken);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            // Use repository to find valid token
            var storedToken = await _refreshTokenRepository.FindValidTokenAsync(userId, refreshToken);
            return storedToken != null;
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expiry)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryDate = expiry,
                CreatedAt = DateTime.UtcNow
            };
            
            // Use repository to add the token
            await _refreshTokenRepository.AddAsync(token);
        }

        public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
        {
            // Use repository to mark token as revoked
            await _refreshTokenRepository.MarkTokenAsRevokedAsync(userId, refreshToken);
        }
    }
}