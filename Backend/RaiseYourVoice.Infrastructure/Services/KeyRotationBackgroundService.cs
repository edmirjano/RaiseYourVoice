using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using System.Security.Cryptography;

namespace RaiseYourVoice.Infrastructure.Services
{
    public class KeyRotationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<KeyRotationBackgroundService> _logger;
        private readonly KeyRotationOptions _options;
        private readonly List<string> _keyPurposes = new List<string> 
        { 
            "DataEncryption", 
            "JwtSigning",
            "ApiPathEncryption" 
        };

        public KeyRotationBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<KeyRotationOptions> options,
            ILogger<KeyRotationBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Key rotation service started");

            // Check immediately on startup
            await CheckAndRotateKeysAsync();

            // Then set up periodic checking
            while (!stoppingToken.IsCancellationRequested)
            {
                // Default check every 24 hours if not specified
                var checkIntervalHours = _options.RotationCheckIntervalHours ?? 24;
                
                try
                {
                    await Task.Delay(TimeSpan.FromHours(checkIntervalHours), stoppingToken);
                    
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await CheckAndRotateKeysAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during key rotation check");
                }
            }
        }

        private async Task CheckAndRotateKeysAsync()
        {
            if (!_options.AutomaticRotation)
            {
                _logger.LogInformation("Automatic key rotation is disabled");
                return;
            }

            _logger.LogInformation("Checking for key rotation needs");

            using var scope = _serviceScopeFactory.CreateScope();
            var keyRepository = scope.ServiceProvider.GetRequiredService<IEncryptionKeyRepository>();
            
            foreach (var purpose in _keyPurposes)
            {
                try
                {
                    await CheckKeyForPurposeAsync(purpose, keyRepository);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking keys for purpose {Purpose}", purpose);
                }
            }
        }

        private async Task CheckKeyForPurposeAsync(string purpose, IEncryptionKeyRepository keyRepository)
        {
            // Get current active key
            var activeKey = await keyRepository.GetActiveKeyAsync(purpose);
            
            if (activeKey == null)
            {
                _logger.LogWarning("No active key found for purpose: {Purpose}. Creating initial key.", purpose);
                await CreateInitialKeyAsync(purpose, keyRepository);
                return;
            }

            // Check if key rotation is needed
            var rotationIntervalDays = _options.RotationIntervalDays ?? 30;
            var rotationThreshold = DateTime.UtcNow.AddDays(-rotationIntervalDays);
            
            if (activeKey.CreatedAt < rotationThreshold)
            {
                _logger.LogInformation("Key rotation needed for purpose: {Purpose}", purpose);
                await RotateKeyAsync(purpose, activeKey, keyRepository);
            }
            else
            {
                _logger.LogInformation("No key rotation needed for purpose: {Purpose}", purpose);
            }
        }

        private async Task CreateInitialKeyAsync(string purpose, IEncryptionKeyRepository keyRepository)
        {
            try
            {
                var key = GenerateEncryptionKey(purpose, 1);
                await keyRepository.AddAsync(key);
                _logger.LogInformation("Created initial key for purpose: {Purpose}", purpose);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create initial key for purpose: {Purpose}", purpose);
                throw;
            }
        }

        private async Task RotateKeyAsync(string purpose, EncryptionKey oldKey, IEncryptionKeyRepository keyRepository)
        {
            try
            {
                // Get highest version
                int highestVersion = await keyRepository.GetHighestVersionAsync(purpose);
                
                // Create new key with incremented version
                var newKey = GenerateEncryptionKey(purpose, highestVersion + 1);
                await keyRepository.AddAsync(newKey);
                
                // Set expiration on old key (after grace period)
                var gracePeriodDays = _options.KeyGracePeriodDays ?? 7;
                oldKey.ExpiresAt = DateTime.UtcNow.AddDays(gracePeriodDays);
                
                // Deactivate old key and activate new key
                await keyRepository.UpdateAsync(oldKey);
                await keyRepository.ActivateKeyAsync(newKey.Id, purpose);
                
                _logger.LogInformation(
                    "Rotated key for purpose: {Purpose}. New key version: {Version}, old key expires: {ExpiryDate}", 
                    purpose, newKey.Version, oldKey.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate key for purpose: {Purpose}", purpose);
                throw;
            }
        }

        private EncryptionKey GenerateEncryptionKey(string purpose, int version)
        {
            byte[] keyBytes;
            
            // Different key generation based on purpose
            switch (purpose)
            {
                case "JwtSigning":
                    // For JWT signing, use asymmetric algorithm
                    using (var rsa = RSA.Create(2048))
                    {
                        return new EncryptionKey
                        {
                            Purpose = purpose,
                            Version = version,
                            KeyData = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
                            PublicKeyData = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                            Algorithm = "RSA-2048",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                
                case "ApiPathEncryption":
                    // For API path encryption, use 128-bit key (shorter for URL efficiency)
                    keyBytes = new byte[16];
                    RandomNumberGenerator.Fill(keyBytes);
                    break;
                
                default:
                    // For data encryption, use 256-bit key
                    keyBytes = new byte[32];
                    RandomNumberGenerator.Fill(keyBytes);
                    break;
            }
            
            return new EncryptionKey
            {
                Purpose = purpose,
                Version = version,
                KeyData = Convert.ToBase64String(keyBytes),
                Algorithm = purpose == "ApiPathEncryption" ? "AES-128" : "AES-256",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Key rotation service is stopping");
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Key rotation service disposed");
            base.Dispose();
        }
    }

    public class KeyRotationOptions
    {
        public int? RotationIntervalDays { get; set; }
        public int? KeyGracePeriodDays { get; set; }
        public bool AutomaticRotation { get; set; } = true;
        public int? RotationCheckIntervalHours { get; set; }
    }
}