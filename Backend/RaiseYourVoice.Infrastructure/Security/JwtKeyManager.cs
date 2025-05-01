using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace RaiseYourVoice.Infrastructure.Security
{
    /// <summary>
    /// Manages JWT signing keys with support for key rotation
    /// </summary>
    public class JwtKeyManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtKeyManager> _logger;
        private readonly ConcurrentDictionary<string, SecurityKey> _signingKeys = new();
        private string _currentKeyId = string.Empty; // Initialize with empty string
        private readonly object _keyRotationLock = new();

        public JwtKeyManager(IConfiguration configuration, ILogger<JwtKeyManager> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Initialize with the default key from configuration
            InitializeDefaultKey();
        }

        public (SecurityKey Key, string KeyId) GetCurrentSigningKey()
        {
            // Ensure we have at least one key
            if (string.IsNullOrEmpty(_currentKeyId) || !_signingKeys.ContainsKey(_currentKeyId))
            {
                lock (_keyRotationLock)
                {
                    if (string.IsNullOrEmpty(_currentKeyId) || !_signingKeys.ContainsKey(_currentKeyId))
                    {
                        InitializeDefaultKey();
                    }
                }
            }

            return (_signingKeys[_currentKeyId], _currentKeyId);
        }

        public IEnumerable<SecurityKey> GetAllSigningKeys()
        {
            return _signingKeys.Values.ToList();
        }

        public bool RotateSigningKey()
        {
            lock (_keyRotationLock)
            {
                try
                {
                    // Generate a new key
                    var newKeyId = $"key-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    var newKey = GenerateNewSigningKey();

                    // Add the new key and update current key ID
                    _signingKeys[newKeyId] = newKey;
                    var oldKeyId = _currentKeyId;
                    _currentKeyId = newKeyId;

                    // Keep the old key for validation but use the new key for signing
                    _logger.LogInformation("JWT signing key rotated from {OldKeyId} to {NewKeyId}", oldKeyId, newKeyId);

                    // Remove keys older than 48 hours (except the current one)
                    var keysToRemove = _signingKeys.Keys
                        .Where(k => k != _currentKeyId && IsKeyOlderThan(k, TimeSpan.FromHours(48)))
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        _signingKeys.TryRemove(key, out _);
                        _logger.LogInformation("Removed old JWT signing key {KeyId}", key);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error rotating JWT signing key");
                    return false;
                }
            }
        }

        private bool IsKeyOlderThan(string keyId, TimeSpan age)
        {
            if (keyId.StartsWith("key-") && long.TryParse(keyId.Substring(4), out long timestamp))
            {
                var keyDate = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                return DateTimeOffset.UtcNow - keyDate > age;
            }

            // If can't parse timestamp, assume old format and remove
            return true;
        }

        private void InitializeDefaultKey()
        {
            // Use the key from configuration as the initial key
            var configKey = _configuration["JwtSettings:SecretKey"] ??
                throw new InvalidOperationException("JWT secret key is not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
            var keyId = "key-config";

            _signingKeys[keyId] = key;
            _currentKeyId = keyId;

            _logger.LogInformation("Initialized default JWT signing key {KeyId}", keyId);
        }

        private SecurityKey GenerateNewSigningKey()
        {
            // Generate a cryptographically strong random key
            var keyBytes = new byte[32]; // 256 bits
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(keyBytes);

            return new SymmetricSecurityKey(keyBytes);
        }
    }
}