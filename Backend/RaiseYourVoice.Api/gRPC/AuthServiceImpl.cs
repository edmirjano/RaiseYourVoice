using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RaiseYourVoice.Api.Protos;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Api.gRPC
{
    public class AuthServiceImpl : AuthService.AuthServiceBase
    {
        private readonly ILogger<AuthServiceImpl> _logger;
        private readonly IGenericRepository<User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthServiceImpl(
            ILogger<AuthServiceImpl> logger,
            IGenericRepository<User> userRepository,
            ITokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _logger = logger;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public override async Task<AuthResponse> Login(LoginRequest request, ServerCallContext context)
        {
            try
            {
                // Find user by email
                var users = await _userRepository.FindAsync(u => u.Email == request.Email);
                var user = users.FirstOrDefault();

                if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid email or password"));
                }

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Generate tokens
                var (token, refreshToken) = await _tokenService.GenerateTokensAsync(user);

                return new AuthResponse
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Token = token,
                    RefreshToken = refreshToken
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred during login"));
            }
        }

        public override async Task<AuthResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            try
            {
                // Check if email already exists
                var existingUsers = await _userRepository.FindAsync(u => u.Email == request.Email);
                if (existingUsers.Any())
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "User with this email already exists"));
                }

                // Create new user
                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    Role = Domain.Enums.UserRole.Activist, // Default role
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

                return new AuthResponse
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Token = token,
                    RefreshToken = refreshToken
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred during registration"));
            }
        }

        public override async Task<AuthResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
        {
            try
            {
                // Get user from token principal
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
                if (principal == null || principal.Identity == null || string.IsNullOrEmpty(principal.Identity.Name))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
                }
                
                var userId = principal.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

                // Validate refresh token
                var isRefreshTokenValid = await _tokenService.ValidateRefreshTokenAsync(userId, request.RefreshToken);
                if (!isRefreshTokenValid)
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid refresh token"));
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
                }

                // Revoke the current refresh token
                await _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);

                // Generate new tokens
                var (token, refreshToken) = await _tokenService.GenerateTokensAsync(user);

                return new AuthResponse
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Token = token,
                    RefreshToken = refreshToken
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while refreshing token"));
            }
        }

        public override async Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
        {
            try
            {
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User identity not found"));
                }
                
                if (!string.IsNullOrEmpty(request.RefreshToken))
                {
                    // Revoke the refresh token
                    await _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);
                }

                return new LogoutResponse
                {
                    Success = true,
                    Message = "Successfully logged out"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred during logout"));
            }
        }

        public override async Task<UserResponse> GetCurrentUser(GetCurrentUserRequest request, ServerCallContext context)
        {
            try
            {
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
                }

                return new UserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    ProfilePicture = user.ProfilePicture ?? "",
                    Bio = user.Bio ?? "",
                    JoinDate = user.JoinDate.ToString("o"),
                    LastLogin = user.LastLogin.ToString("o"),
                    PreferredLanguage = user.Preferences?.PreferredLanguage ?? "en",
                    NotificationSettings = new NotificationSettingsModel
                    {
                        EmailNotifications = user.Preferences?.NotificationSettings?.EmailNotifications ?? true,
                        PushNotifications = user.Preferences?.NotificationSettings?.PushNotifications ?? true,
                        NewPostNotifications = user.Preferences?.NotificationSettings?.NewPostNotifications ?? true,
                        CommentNotifications = user.Preferences?.NotificationSettings?.CommentNotifications ?? true,
                        EventReminders = user.Preferences?.NotificationSettings?.EventReminders ?? true,
                        PreferredNotificationTime = user.Preferences?.NotificationSettings?.PreferredNotificationTime ?? "09:00"
                    }
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while getting current user"));
            }
        }
    }
}