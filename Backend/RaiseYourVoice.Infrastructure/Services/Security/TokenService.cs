using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace RaiseYourVoice.Infrastructure.Services.Security
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<RefreshToken> _refreshTokens;

        public TokenService(IConfiguration configuration, IMongoClient mongoClient)
        {
            _configuration = configuration;
            var database = mongoClient.GetDatabase("RaiseYourVoice");
            _refreshTokens = database.GetCollection<RefreshToken>("RefreshTokens");
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT secret key is not configured");
            var key = Encoding.UTF8.GetBytes(keyString);
            
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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
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
                var secretKey = _configuration["JwtSettings:SecretKey"];
                
                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("JWT secret key is not configured");
                }
                
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // Don't validate lifetime for expired tokens
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
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
            
            // Store refresh token
            await SaveRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));
            
            return (accessToken, refreshToken);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var storedToken = await _refreshTokens
                .Find(rt => rt.UserId == userId && rt.Token == refreshToken && rt.ExpiryDate > DateTime.UtcNow && !rt.IsRevoked)
                .FirstOrDefaultAsync();
            
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
            
            await _refreshTokens.InsertOneAsync(token);
        }

        public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
        {
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow)
                .Set(rt => rt.UpdatedAt, DateTime.UtcNow);
            
            await _refreshTokens.UpdateOneAsync(
                rt => rt.UserId == userId && rt.Token == refreshToken,
                update);
        }
    }
}