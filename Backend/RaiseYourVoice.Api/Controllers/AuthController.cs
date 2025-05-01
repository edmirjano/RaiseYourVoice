using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Application.Models.Responses;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthController(
            IGenericRepository<User> userRepository,
            ITokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            // Check if email already exists
            var existingUsers = await _userRepository.FindAsync(u => u.Email == request.Email);
            if (existingUsers.Any())
            {
                return BadRequest("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = UserRole.Activist, // Default role
                ProfilePicture = request.ProfilePicture,
                Bio = request.Bio,
                JoinDate = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                Preferences = new UserPreferences
                {
                    PreferredLanguage = request.PreferredLanguage,
                    NotificationSettings = new NotificationSettings(),
                },
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Generate tokens
            var (token, refreshToken) = await _tokenService.GenerateTokensAsync(user);

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            // Find user by email
            var users = await _userRepository.FindAsync(u => u.Email == request.Email);
            var user = users.FirstOrDefault();

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate tokens
            var (token, refreshToken) = await _tokenService.GenerateTokensAsync(user);

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                // Get user from token principal
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
                if (principal == null || principal.Identity == null || string.IsNullOrEmpty(principal.Identity.Name))
                {
                    return Unauthorized("Invalid token");
                }
                
                var userId = principal.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

                // Validate refresh token
                var isRefreshTokenValid = await _tokenService.ValidateRefreshTokenAsync(userId, request.RefreshToken);
                if (!isRefreshTokenValid)
                {
                    return Unauthorized("Invalid refresh token");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                // Revoke the current refresh token
                await _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);

                // Generate new tokens
                var (token, refreshToken) = await _tokenService.GenerateTokensAsync(user);

                return Ok(new AuthResponse
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    Token = token,
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User identity not found");
            }
            
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                // Revoke the refresh token
                await _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);
            }

            return Ok(new { message = "Successfully logged out" });
        }
    }
}