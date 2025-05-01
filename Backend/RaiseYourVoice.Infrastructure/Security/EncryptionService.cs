using Microsoft.Extensions.Configuration;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RaiseYourVoice.Infrastructure.Security
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data using AES-256
    /// with support for key rotation
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly IEncryptionKeyRepository _keyRepository;
        private readonly ILogger<EncryptionService> _logger;
        private readonly EncryptionLoggingService _encryptionLogger;

        // Format is {keyVersion}:{encryptedText}
        private static readonly Regex _encryptedFormatRegex = new Regex(@"^(\d+):(.+)$", RegexOptions.Compiled);
        
        // Cache of decryption keys to avoid frequent database calls
        private readonly Dictionary<string, Dictionary<int, EncryptionKey>> _keyCache = new Dictionary<string, Dictionary<int, EncryptionKey>>();

        public EncryptionService(
            IConfiguration configuration,
            IEncryptionKeyRepository keyRepository = null,
            EncryptionLoggingService encryptionLogger = null,
            ILogger<EncryptionService> logger = null)
        {
            _configuration = configuration;
            _keyRepository = keyRepository;
            _encryptionLogger = encryptionLogger;
            _logger = logger;
        }

        /// <summary>
        /// Encrypts a string value using the current active key
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            // Handle two modes:
            // 1. With repository: Use managed keys from the database
            // 2. Without repository: Use keys from configuration

            if (_keyRepository != null)
            {
                return EncryptWithKeyRepository(plainText, "field-encryption").GetAwaiter().GetResult();
            }
            else
            {
                return EncryptWithConfigKey(plainText);
            }
        }

        /// <summary>
        /// Decrypts an encrypted string value
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            try
            {
                // Check if the cipher text is in the versioned format
                var match = _encryptedFormatRegex.Match(cipherText);
                
                if (match.Success)
                {
                    int keyVersion = int.Parse(match.Groups[1].Value);
                    string encryptedValue = match.Groups[2].Value;
                    
                    if (_keyRepository != null)
                    {
                        return DecryptWithKeyRepository(encryptedValue, keyVersion, "field-encryption").GetAwaiter().GetResult();
                    }
                    else
                    {
                        // For fallback/backwards compatibility
                        return DecryptWithConfigKey(encryptedValue);
                    }
                }
                else
                {
                    // For backwards compatibility with non-versioned encrypted values
                    return DecryptWithConfigKey(cipherText);
                }
            }
            catch (CryptographicException ex)
            {
                _encryptionLogger?.LogSecurityEvent(
                    "DecryptionFailure", 
                    "Failed to decrypt value. It may be corrupted or tampered with.", 
                    severity: 2);
                
                _logger?.LogError(ex, "Failed to decrypt value. It may be corrupted or tampered with.");
                throw new CryptographicException("Failed to decrypt value. It may be corrupted or tampered with.", ex);
            }
            catch (FormatException ex)
            {
                _encryptionLogger?.LogSecurityEvent(
                    "DecryptionFormatError", 
                    "The encrypted data was not in the correct format.", 
                    severity: 1);
                
                _logger?.LogError(ex, "The encrypted data was not in the correct format.");
                throw new FormatException("The encrypted data was not in the correct format.", ex);
            }
            catch (Exception ex)
            {
                _encryptionLogger?.LogSecurityEvent(
                    "DecryptionGeneralError", 
                    $"An unexpected error occurred during decryption: {ex.Message}", 
                    severity: 2);
                
                _logger?.LogError(ex, "An unexpected error occurred during decryption.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously encrypts a string value
        /// </summary>
        public async Task<string> EncryptAsync(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            if (_keyRepository != null)
            {
                return await EncryptWithKeyRepository(plainText, "field-encryption");
            }
            else
            {
                return EncryptWithConfigKey(plainText);
            }
        }

        /// <summary>
        /// Asynchronously decrypts an encrypted string value
        /// </summary>
        public async Task<string> DecryptAsync(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            try
            {
                // Check if the cipher text is in the versioned format
                var match = _encryptedFormatRegex.Match(cipherText);
                
                if (match.Success)
                {
                    int keyVersion = int.Parse(match.Groups[1].Value);
                    string encryptedValue = match.Groups[2].Value;
                    
                    if (_keyRepository != null)
                    {
                        return await DecryptWithKeyRepository(encryptedValue, keyVersion, "field-encryption");
                    }
                    else
                    {
                        // For fallback/backwards compatibility
                        return DecryptWithConfigKey(encryptedValue);
                    }
                }
                else
                {
                    // For backwards compatibility with non-versioned encrypted values
                    return DecryptWithConfigKey(cipherText);
                }
            }
            catch (Exception ex)
            {
                _encryptionLogger?.LogSecurityEvent(
                    "AsyncDecryptionError", 
                    $"Failed to decrypt value: {ex.Message}", 
                    severity: 2);
                    
                _logger?.LogError(ex, "Failed to decrypt value");
                throw;
            }
        }

        /// <summary>
        /// Gets the current active encryption key for a specific purpose
        /// </summary>
        public async Task<EncryptionKey> GetActiveKeyAsync(string purpose = "field-encryption")
        {
            if (_keyRepository == null)
            {
                throw new InvalidOperationException("Key repository is not available for key rotation operations");
            }
            
            // Try to get active key from database
            var activeKey = await _keyRepository.GetActiveKeyAsync(purpose);
            
            // If no active key exists, create one
            if (activeKey == null)
            {
                _encryptionLogger?.LogSecurityEvent(
                    "NoActiveKeyFound", 
                    $"No active key found for purpose {purpose}, creating new key",
                    severity: 1);
                
                _logger?.LogInformation("No active key found for purpose {Purpose}, creating new key", purpose);
                activeKey = await GenerateNewKeyAsync(purpose, true);
            }
            
            // Add to cache
            if (!_keyCache.ContainsKey(purpose))
            {
                _keyCache[purpose] = new Dictionary<int, EncryptionKey>();
            }
            
            _keyCache[purpose][activeKey.Version] = activeKey;
            
            return activeKey;
        }

        /// <summary>
        /// Gets an encryption key by version
        /// </summary>
        public async Task<EncryptionKey> GetKeyByVersionAsync(int version, string purpose = "field-encryption")
        {
            if (_keyRepository == null)
            {
                throw new InvalidOperationException("Key repository is not available for key rotation operations");
            }

            // Check cache first
            if (_keyCache.ContainsKey(purpose) && _keyCache[purpose].ContainsKey(version))
            {
                return _keyCache[purpose][version];
            }
            
            // Get from database
            var key = await _keyRepository.GetKeyByVersionAsync(version, purpose);
            
            // Add to cache if found
            if (key != null)
            {
                if (!_keyCache.ContainsKey(purpose))
                {
                    _keyCache[purpose] = new Dictionary<int, EncryptionKey>();
                }
                
                _keyCache[purpose][version] = key;
            }
            else
            {
                _encryptionLogger?.LogSecurityEvent(
                    "KeyVersionNotFound", 
                    $"Encryption key version {version} for purpose {purpose} not found", 
                    severity: 2);
            }
            
            return key;
        }

        /// <summary>
        /// Generates and stores a new encryption key
        /// </summary>
        public async Task<EncryptionKey> GenerateNewKeyAsync(string purpose = "field-encryption", bool activateImmediately = false, int expiresInDays = 90)
        {
            if (_keyRepository == null)
            {
                throw new InvalidOperationException("Key repository is not available for key rotation operations");
            }
            
            // Generate a new AES key
            using var aes = Aes.Create();
            aes.KeySize = 256; // AES-256
            aes.GenerateKey();
            aes.GenerateIV();
            
            // Get the next version number
            int version = await _keyRepository.GetHighestVersionAsync(purpose) + 1;
            
            // Create new key entity
            var newKey = new EncryptionKey
            {
                Version = version,
                Key = Convert.ToBase64String(aes.Key),
                IV = Convert.ToBase64String(aes.IV),
                CreatedAt = DateTime.UtcNow,
                ActivatedAt = activateImmediately ? DateTime.UtcNow : DateTime.UtcNow.AddDays(1), // Delay activation by default
                ExpiresAt = DateTime.UtcNow.AddDays(expiresInDays),
                IsActive = activateImmediately,
                Purpose = purpose,
                Description = $"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}"
            };
            
            // Save to database
            await _keyRepository.AddAsync(newKey);
            
            // Log the key generation
            _encryptionLogger?.LogKeyGeneration(
                purpose, 
                version, 
                newKey.ActivatedAt, 
                newKey.ExpiresAt);
            
            // If activating immediately, deactivate all other keys
            if (activateImmediately)
            {
                await _keyRepository.ActivateKeyAsync(newKey.Id, purpose);
                
                // Log key activation
                _encryptionLogger?.LogKeyActivation(purpose, version);
                
                // Clear cache
                if (_keyCache.ContainsKey(purpose))
                {
                    _keyCache[purpose].Clear();
                }
            }
            
            _logger?.LogInformation("Generated new {Purpose} key with version {Version}", purpose, version);
            
            return newKey;
        }

        /// <summary>
        /// Activates a specific encryption key version
        /// </summary>
        public async Task<bool> ActivateKeyVersionAsync(int version, string purpose = "field-encryption")
        {
            if (_keyRepository == null)
            {
                throw new InvalidOperationException("Key repository is not available for key rotation operations");
            }
            
            var key = await _keyRepository.GetKeyByVersionAsync(version, purpose);
            
            if (key == null)
            {
                _encryptionLogger?.LogSecurityEvent(
                    "KeyActivationFailed", 
                    $"Cannot activate key version {version} for purpose {purpose}: key not found", 
                    severity: 1);
                
                _logger?.LogWarning("Cannot activate key version {Version} for purpose {Purpose}: key not found", version, purpose);
                return false;
            }
            
            // Get the current active key for rotation logging
            var activeKey = await _keyRepository.GetActiveKeyAsync(purpose);
            int oldVersion = activeKey?.Version ?? 0;
            
            // Activate key
            bool success = await _keyRepository.ActivateKeyAsync(key.Id, purpose);
            
            if (success)
            {
                // Log key rotation
                _encryptionLogger?.LogKeyRotation(purpose, oldVersion, version);
                
                // Clear cache
                if (_keyCache.ContainsKey(purpose))
                {
                    _keyCache[purpose].Clear();
                }
                
                _logger?.LogInformation("Activated {Purpose} key version {Version}", purpose, version);
            }
            
            return success;
        }

        /// <summary>
        /// Gets all encryption keys for a specific purpose
        /// </summary>
        public async Task<List<EncryptionKey>> GetKeysAsync(string purpose = "field-encryption", bool includeExpired = false)
        {
            if (_keyRepository == null)
            {
                throw new InvalidOperationException("Key repository is not available for key rotation operations");
            }
            
            return await _keyRepository.GetKeysByPurposeAsync(purpose, includeExpired);
        }

        /// <summary>
        /// Performs scheduled key rotation check and generates new keys if needed
        /// </summary>
        public async Task<bool> PerformScheduledKeyRotationAsync()
        {
            if (_keyRepository == null)
            {
                throw new InvalidOperationException("Key repository is not available for key rotation operations");
            }
            
            bool anyRotated = false;
            
            // Get key rotation settings from configuration
            var rotationDays = _configuration.GetValue<int>("SecuritySettings:EncryptionSettings:KeyRotationDays", 30);
            var purposes = new[] { "field-encryption", "api-path" };
            
            foreach (var purpose in purposes)
            {
                // Get active key
                var activeKey = await _keyRepository.GetActiveKeyAsync(purpose);
                
                // If no active key or key is nearing expiration, generate a new one
                if (activeKey == null || activeKey.ExpiresAt < DateTime.UtcNow.AddDays(rotationDays * 0.2))
                {
                    _encryptionLogger?.LogSecurityEvent(
                        "ScheduledKeyGeneration", 
                        $"Performing scheduled key generation for {purpose}", 
                        severity: 0);
                    
                    _logger?.LogInformation("Performing scheduled key rotation for {Purpose}", purpose);
                    
                    // Generate new key but don't activate it yet
                    await GenerateNewKeyAsync(purpose, false, rotationDays);
                    anyRotated = true;
                }
                
                // If active key created more than rotationDays*0.8 days ago, activate the next key
                if (activeKey != null && activeKey.CreatedAt < DateTime.UtcNow.AddDays(-rotationDays * 0.8))
                {
                    // Find the newest non-active key
                    var keys = await _keyRepository.GetKeysByPurposeAsync(purpose, false);
                    var nextKey = keys.Find(k => !k.IsActive && k.Version > activeKey.Version);
                    
                    if (nextKey != null)
                    {
                        _encryptionLogger?.LogSecurityEvent(
                            "ScheduledKeyActivation", 
                            $"Activating next key version {nextKey.Version} for {purpose}", 
                            severity: 0);
                        
                        _logger?.LogInformation("Activating next key version {Version} for {Purpose}", nextKey.Version, purpose);
                        await _keyRepository.ActivateKeyAsync(nextKey.Id, purpose);
                        
                        // Log key rotation
                        _encryptionLogger?.LogKeyRotation(purpose, activeKey.Version, nextKey.Version);
                        
                        anyRotated = true;
                        
                        // Clear cache
                        if (_keyCache.ContainsKey(purpose))
                        {
                            _keyCache[purpose].Clear();
                        }
                    }
                }
            }
            
            return anyRotated;
        }

        #region Private Helper Methods

        private string EncryptWithConfigKey(string plainText)
        {
            // Get encryption settings from configuration
            var encryptionKey = _configuration["EncryptionSettings:Key"];
            var encryptionIv = _configuration["EncryptionSettings:IV"];
            
            if (string.IsNullOrEmpty(encryptionKey) || string.IsNullOrEmpty(encryptionIv))
            {
                throw new ArgumentNullException("Encryption key or IV is not configured");
            }

            // Convert the key and IV from base64 to byte arrays
            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = Convert.FromBase64String(encryptionIv);

            // Track encryption operation for performance logging
            if (_encryptionLogger != null)
            {
                return _encryptionLogger.TrackOperation<string>(
                    "Encrypt", 
                    "config", 
                    0, 
                    () => PerformEncryption(plainText, key, iv));
            }
            
            return PerformEncryption(plainText, key, iv);
        }

        private string DecryptWithConfigKey(string cipherText)
        {
            // Get encryption settings from configuration
            var encryptionKey = _configuration["EncryptionSettings:Key"];
            var encryptionIv = _configuration["EncryptionSettings:IV"];
            
            if (string.IsNullOrEmpty(encryptionKey) || string.IsNullOrEmpty(encryptionIv))
            {
                throw new ArgumentNullException("Encryption key or IV is not configured");
            }

            // Convert the key and IV from base64 to byte arrays
            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = Convert.FromBase64String(encryptionIv);
            
            // Track decryption operation for performance logging
            if (_encryptionLogger != null)
            {
                return _encryptionLogger.TrackOperation<string>(
                    "Decrypt", 
                    "config", 
                    0, 
                    () => PerformDecryption(cipherText, key, iv));
            }
            
            return PerformDecryption(cipherText, key, iv);
        }

        private async Task<string> EncryptWithKeyRepository(string plainText, string purpose)
        {
            // Get the active key for the specified purpose
            var activeKey = await GetActiveKeyAsync(purpose);
            
            // Convert the key and IV from base64 to byte arrays
            byte[] key = Convert.FromBase64String(activeKey.Key);
            byte[] iv = Convert.FromBase64String(activeKey.IV);

            string encryptedValue;
            
            // Track encryption operation for performance logging
            if (_encryptionLogger != null)
            {
                encryptedValue = await _encryptionLogger.TrackOperationAsync<string>(
                    "Encrypt", 
                    purpose, 
                    activeKey.Version, 
                    async () => PerformEncryption(plainText, key, iv));
            }
            else
            {
                encryptedValue = PerformEncryption(plainText, key, iv);
            }

            // Format: {keyVersion}:{encryptedBase64}
            return $"{activeKey.Version}:{encryptedValue}";
        }

        private async Task<string> DecryptWithKeyRepository(string encryptedValue, int keyVersion, string purpose)
        {
            // Get the key with the specified version
            var key = await GetKeyByVersionAsync(keyVersion, purpose);
            
            if (key == null)
            {
                _encryptionLogger?.LogSecurityEvent(
                    "KeyNotFoundForDecryption", 
                    $"Encryption key version {keyVersion} for purpose {purpose} not found", 
                    severity: 2);
                
                throw new CryptographicException($"Encryption key version {keyVersion} for purpose {purpose} not found");
            }
            
            // Convert the key and IV from base64 to byte arrays
            byte[] keyBytes = Convert.FromBase64String(key.Key);
            byte[] ivBytes = Convert.FromBase64String(key.IV);
            
            // Track decryption operation for performance logging
            if (_encryptionLogger != null)
            {
                return await _encryptionLogger.TrackOperationAsync<string>(
                    "Decrypt", 
                    purpose, 
                    keyVersion, 
                    async () => PerformDecryption(encryptedValue, keyBytes, ivBytes));
            }
            
            return PerformDecryption(encryptedValue, keyBytes, ivBytes);
        }

        /// <summary>
        /// Core encryption logic extracted for reuse
        /// </summary>
        private string PerformEncryption(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Core decryption logic extracted for reuse
        /// </summary>
        private string PerformDecryption(string cipherText, byte[] key, byte[] iv)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return sr.ReadToEnd();
        }

        #endregion
    }
}