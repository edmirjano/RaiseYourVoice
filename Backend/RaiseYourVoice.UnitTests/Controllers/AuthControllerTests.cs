using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RaiseYourVoice.Api.Controllers;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using Xunit;

namespace RaiseYourVoice.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IGenericRepository<User>> _mockUserRepository;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        
        public AuthControllerTests()
        {
            _mockUserRepository = new Mock<IGenericRepository<User>>();
            _mockTokenService = new Mock<ITokenService>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
        }
        
        [Fact]
        public async Task Register_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var controller = new AuthController(
                _mockUserRepository.Object,
                _mockTokenService.Object,
                _mockPasswordHasher.Object
            );
            
            var request = new RegisterRequest
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                PreferredLanguage = "en"
            };
            
            _mockUserRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User>());
                
            _mockPasswordHasher.Setup(h => h.HashPassword(request.Password))
                .Returns("hashed_password");
                
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(new User
                {
                    Id = "user1",
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = "hashed_password",
                    Role = UserRole.Activist
                });
                
            _mockTokenService.Setup(t => t.GenerateTokensAsync(It.IsAny<User>()))
                .ReturnsAsync(("token", "refresh_token"));
            
            // Act
            var result = await controller.Register(request);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Application.Models.Responses.AuthResponse>(okResult.Value);
            Assert.Equal("user1", response.UserId);
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(UserRole.Activist, response.Role);
            Assert.Equal("token", response.Token);
            Assert.Equal("refresh_token", response.RefreshToken);
        }
        
        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var controller = new AuthController(
                _mockUserRepository.Object,
                _mockTokenService.Object,
                _mockPasswordHasher.Object
            );
            
            var request = new RegisterRequest
            {
                Name = "Test User",
                Email = "existing@example.com",
                Password = "Password123!",
                PreferredLanguage = "en"
            };
            
            _mockUserRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { new User { Email = "existing@example.com" } });
            
            // Act
            var result = await controller.Register(request);
            
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("User with this email already exists", badRequestResult.Value);
        }
        
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var controller = new AuthController(
                _mockUserRepository.Object,
                _mockTokenService.Object,
                _mockPasswordHasher.Object
            );
            
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };
            
            var user = new User
            {
                Id = "user1",
                Name = "Test User",
                Email = request.Email,
                PasswordHash = "hashed_password",
                Role = UserRole.Activist
            };
            
            _mockUserRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });
                
            _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(true);
                
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(true);
                
            _mockTokenService.Setup(t => t.GenerateTokensAsync(It.IsAny<User>()))
                .ReturnsAsync(("token", "refresh_token"));
            
            // Act
            var result = await controller.Login(request);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Application.Models.Responses.AuthResponse>(okResult.Value);
            Assert.Equal("user1", response.UserId);
            Assert.Equal("Test User", response.Name);
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(UserRole.Activist, response.Role);
            Assert.Equal("token", response.Token);
            Assert.Equal("refresh_token", response.RefreshToken);
        }
        
        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AuthController(
                _mockUserRepository.Object,
                _mockTokenService.Object,
                _mockPasswordHasher.Object
            );
            
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };
            
            var user = new User
            {
                Id = "user1",
                Name = "Test User",
                Email = request.Email,
                PasswordHash = "hashed_password",
                Role = UserRole.Activist
            };
            
            _mockUserRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });
                
            _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(false);
            
            // Act
            var result = await controller.Login(request);
            
            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid email or password", unauthorizedResult.Value);
        }
        
        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsOkResult()
        {
            // Arrange
            var controller = new AuthController(
                _mockUserRepository.Object,
                _mockTokenService.Object,
                _mockPasswordHasher.Object
            );
            
            var request = new RefreshTokenRequest
            {
                Token = "expired_token",
                RefreshToken = "valid_refresh_token"
            };
            
            var user = new User
            {
                Id = "user1",
                Name = "Test User",
                Email = "test@example.com",
                Role = UserRole.Activist
            };
            
            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new System.Security.Claims.Claim[] 
                    {
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user1")
                    },
                    "test"
                )
            );
            
            _mockTokenService.Setup(t => t.GetPrincipalFromExpiredToken(request.Token))
                .Returns(principal);
                
            _mockTokenService.Setup(t => t.ValidateRefreshTokenAsync("user1", request.RefreshToken))
                .ReturnsAsync(true);
                
            _mockUserRepository.Setup(r => r.GetByIdAsync("user1"))
                .ReturnsAsync(user);
                
            _mockTokenService.Setup(t => t.RevokeRefreshTokenAsync("user1", request.RefreshToken))
                .Returns(Task.CompletedTask);
                
            _mockTokenService.Setup(t => t.GenerateTokensAsync(user))
                .ReturnsAsync(("new_token", "new_refresh_token"));
            
            // Act
            var result = await controller.RefreshToken(request);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Application.Models.Responses.AuthResponse>(okResult.Value);
            Assert.Equal("user1", response.UserId);
            Assert.Equal("Test User", response.Name);
            Assert.Equal("test@example.com", response.Email);
            Assert.Equal(UserRole.Activist, response.Role);
            Assert.Equal("new_token", response.Token);
            Assert.Equal("new_refresh_token", response.RefreshToken);
        }
    }
}