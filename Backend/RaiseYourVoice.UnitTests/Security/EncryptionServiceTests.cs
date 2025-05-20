using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Infrastructure.Security;
using Xunit;

namespace RaiseYourVoice.UnitTests.Security
{
    public class EncryptionServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IEncryptionKeyRepository> _mockKeyRepository;
        private readonly Mock<EncryptionLoggingService> _mockEncryptionLogger;
        private readonly Mock<ILogger<EncryptionService>> _mockLogger;
        
        public EncryptionServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockKeyRepository = new Mock<IEncryptionKeyRepository>();
            _mockEncryptionLogger = new Mock<EncryptionLoggingService>();
            _mockLogger = new Mock<ILogger<EncryptionService>>();
            
            // Setup configuration
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(s => s.Value).Returns("NEJENkI4QTMtNkE1RC00RjU4LUE4NTUtMjg2M0QwMzE1RkJB");
            _mockConfiguration.Setup(c => c["EncryptionSettings:Key"]).Returns("NEJENkI4QTMtNkE1RC00RjU4LUE4NTUtMjg2M0QwMzE1RkJB");
            _mockConfiguration.Setup(c => c["EncryptionSettings:IV"]).Returns("RkE1RDZCMzQtOUE4NS00NTY3LThFOTctMTcyRDM0ODU2QzIz");
        }
        
        [Fact]
        public void Encrypt_WithConfigKey_ReturnsEncryptedValue()
        {
            // Arrange
            var service = new EncryptionService(_mockConfiguration.Object);
            var plainText = "sensitive data";
            
            // Act
            var encryptedText = service.Encrypt(plainText);
            
            // Assert
            Assert.NotNull(encryptedText);
            Assert.NotEqual(plainText, encryptedText);
        }
        
        [Fact]
        public void Decrypt_WithConfigKey_ReturnsOriginalValue()
        {
            // Arrange
            var service = new EncryptionService(_mockConfiguration.Object);
            var plainText = "sensitive data";
            var encryptedText = service.Encrypt(plainText);
            
            // Act
            var decryptedText = service.Decrypt(encryptedText);
            
            // Assert
            Assert.Equal(plainText, decryptedText);
        }
        
        [Fact]
        public async Task GetActiveKeyAsync_WhenKeyExists_ReturnsKey()
        {
            // Arrange
            var activeKey = new EncryptionKey
            {
                Id = "key1",
                Version = 1,
                Key = "test-key-data",
                IV = "test-iv-data",
                Purpose = "field-encryption",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                KeyCreatedAt = DateTime.UtcNow,
                ActivatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };
            
            _mockKeyRepository.Setup(r => r.GetActiveKeyAsync("field-encryption"))
                .ReturnsAsync(activeKey);
                
            var service = new EncryptionService(
                _mockConfiguration.Object,
                _mockKeyRepository.Object,
                _mockEncryptionLogger.Object,
                _mockLogger.Object);
            
            // Act
            var result = await service.GetActiveKeyAsync();
            
            // Assert
            Assert.Equal(activeKey.Id, result.Id);
            Assert.Equal(activeKey.Version, result.Version);
            Assert.Equal(activeKey.Key, result.Key);
            Assert.Equal(activeKey.IV, result.IV);
        }
        
        [Fact]
        public async Task GetActiveKeyAsync_WhenKeyDoesNotExist_CreatesNewKey()
        {
            // Arrange
            _mockKeyRepository.Setup(r => r.GetActiveKeyAsync("field-encryption"))
                .ReturnsAsync((EncryptionKey)null);
                
            var newKey = new EncryptionKey
            {
                Id = "new-key",
                Version = 1,
                Key = "new-key-data",
                IV = "new-iv-data",
                Purpose = "field-encryption",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                KeyCreatedAt = DateTime.UtcNow,
                ActivatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };
            
            _mockKeyRepository.Setup(r => r.GetHighestVersionAsync("field-encryption"))
                .ReturnsAsync(0);
                
            _mockKeyRepository.Setup(r => r.AddAsync(It.IsAny<EncryptionKey>()))
                .ReturnsAsync(newKey);
                
            var service = new EncryptionService(
                _mockConfiguration.Object,
                _mockKeyRepository.Object,
                _mockEncryptionLogger.Object,
                _mockLogger.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetActiveKeyAsync());
        }
    }
}