using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using RaiseYourVoice.Infrastructure.Security;
using Xunit;

namespace RaiseYourVoice.UnitTests.Security
{
    public class JwtKeyManagerTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<JwtKeyManager>> _mockLogger;
        
        public JwtKeyManagerTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<JwtKeyManager>>();
            
            // Setup configuration
            _mockConfiguration.Setup(c => c["JwtSettings:SecretKey"]).Returns("your-super-secret-key-for-raiseYourVoice-platform");
        }
        
        [Fact]
        public void GetCurrentSigningKey_ReturnsValidKey()
        {
            // Arrange
            var keyManager = new JwtKeyManager(_mockConfiguration.Object, _mockLogger.Object);
            
            // Act
            var (key, keyId) = keyManager.GetCurrentSigningKey();
            
            // Assert
            Assert.NotNull(key);
            Assert.NotNull(keyId);
            Assert.IsType<SymmetricSecurityKey>(key);
        }
        
        [Fact]
        public void GetAllSigningKeys_ReturnsAtLeastOneKey()
        {
            // Arrange
            var keyManager = new JwtKeyManager(_mockConfiguration.Object, _mockLogger.Object);
            
            // Act
            var keys = keyManager.GetAllSigningKeys();
            
            // Assert
            Assert.NotNull(keys);
            Assert.NotEmpty(keys);
        }
        
        [Fact]
        public void RotateSigningKey_CreatesNewKey()
        {
            // Arrange
            var keyManager = new JwtKeyManager(_mockConfiguration.Object, _mockLogger.Object);
            var (originalKey, originalKeyId) = keyManager.GetCurrentSigningKey();
            
            // Act
            var result = keyManager.RotateSigningKey();
            var (newKey, newKeyId) = keyManager.GetCurrentSigningKey();
            
            // Assert
            Assert.True(result);
            Assert.NotEqual(originalKeyId, newKeyId);
            
            // Keys should be different, but both should be valid SymmetricSecurityKey instances
            Assert.NotEqual(
                Convert.ToBase64String(((SymmetricSecurityKey)originalKey).Key),
                Convert.ToBase64String(((SymmetricSecurityKey)newKey).Key)
            );
        }
        
        [Fact]
        public void RotateSigningKey_OldKeyStillAvailableForValidation()
        {
            // Arrange
            var keyManager = new JwtKeyManager(_mockConfiguration.Object, _mockLogger.Object);
            var (originalKey, originalKeyId) = keyManager.GetCurrentSigningKey();
            
            // Act
            keyManager.RotateSigningKey();
            var allKeys = keyManager.GetAllSigningKeys();
            
            // Assert
            // The original key should still be in the collection of all keys
            Assert.Contains(allKeys, k => 
                Convert.ToBase64String(((SymmetricSecurityKey)k).Key) == 
                Convert.ToBase64String(((SymmetricSecurityKey)originalKey).Key)
            );
        }
    }
}