using System;
using Microsoft.Extensions.Configuration;
using Moq;
using RaiseYourVoice.Infrastructure.Services.Security;
using Xunit;

namespace RaiseYourVoice.UnitTests.Security
{
    public class PasswordHasherTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        
        public PasswordHasherTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["SecuritySettings:PasswordHashingIterations"]).Returns("10000");
        }
        
        [Fact]
        public void HashPassword_ReturnsHashedPassword()
        {
            // Arrange
            var hasher = new PasswordHasher(_mockConfiguration.Object);
            var password = "SecurePassword123!";
            
            // Act
            var hashedPassword = hasher.HashPassword(password);
            
            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotEqual(password, hashedPassword);
        }
        
        [Fact]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            var hasher = new PasswordHasher(_mockConfiguration.Object);
            var password = "SecurePassword123!";
            var hashedPassword = hasher.HashPassword(password);
            
            // Act
            var result = hasher.VerifyPassword(password, hashedPassword);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var hasher = new PasswordHasher(_mockConfiguration.Object);
            var password = "SecurePassword123!";
            var incorrectPassword = "WrongPassword456!";
            var hashedPassword = hasher.HashPassword(password);
            
            // Act
            var result = hasher.VerifyPassword(incorrectPassword, hashedPassword);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void HashPassword_WithDifferentPasswords_CreatesDifferentHashes()
        {
            // Arrange
            var hasher = new PasswordHasher(_mockConfiguration.Object);
            var password1 = "SecurePassword123!";
            var password2 = "AnotherPassword456!";
            
            // Act
            var hash1 = hasher.HashPassword(password1);
            var hash2 = hasher.HashPassword(password2);
            
            // Assert
            Assert.NotEqual(hash1, hash2);
        }
        
        [Fact]
        public void HashPassword_WithSamePassword_CreatesDifferentHashes()
        {
            // Arrange
            var hasher = new PasswordHasher(_mockConfiguration.Object);
            var password = "SecurePassword123!";
            
            // Act
            var hash1 = hasher.HashPassword(password);
            var hash2 = hasher.HashPassword(password);
            
            // Assert
            Assert.NotEqual(hash1, hash2); // Due to different salts
        }
    }
}