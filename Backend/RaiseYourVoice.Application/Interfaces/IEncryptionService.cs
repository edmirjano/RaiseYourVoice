using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Application.Interfaces
{
    /// <summary>
    /// Interface for encrypting and decrypting sensitive data
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a string value using the current active key
        /// </summary>
        /// <param name="plainText">The plain text to encrypt</param>
        /// <returns>The encrypted value with key version metadata</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts an encrypted string value
        /// </summary>
        /// <param name="cipherText">The encrypted text with key version metadata</param>
        /// <returns>The decrypted plain text</returns>
        string Decrypt(string cipherText);

        /// <summary>
        /// Asynchronously encrypts a string value using the current active key
        /// </summary>
        /// <param name="plainText">The plain text to encrypt</param>
        /// <returns>The encrypted value with key version metadata</returns>
        Task<string> EncryptAsync(string plainText);

        /// <summary>
        /// Asynchronously decrypts an encrypted string value
        /// </summary>
        /// <param name="cipherText">The encrypted text with key version metadata</param>
        /// <returns>The decrypted plain text</returns>
        Task<string> DecryptAsync(string cipherText);
        
        /// <summary>
        /// Gets the current active encryption key for a specific purpose
        /// </summary>
        /// <param name="purpose">The purpose of the key (default is "field-encryption")</param>
        /// <returns>The active encryption key</returns>
        Task<EncryptionKey> GetActiveKeyAsync(string purpose = "field-encryption");
        
        /// <summary>
        /// Gets an encryption key by version
        /// </summary>
        /// <param name="version">The key version</param>
        /// <param name="purpose">The purpose of the key</param>
        /// <returns>The encryption key with the specified version</returns>
        Task<EncryptionKey> GetKeyByVersionAsync(int version, string purpose = "field-encryption");
        
        /// <summary>
        /// Generates and stores a new encryption key
        /// </summary>
        /// <param name="purpose">The purpose of the key (default is "field-encryption")</param>
        /// <param name="activateImmediately">Whether to activate the new key immediately</param>
        /// <param name="expiresInDays">How many days until the key expires</param>
        /// <returns>The newly generated encryption key</returns>
        Task<EncryptionKey> GenerateNewKeyAsync(string purpose = "field-encryption", 
            bool activateImmediately = false, int expiresInDays = 90);
        
        /// <summary>
        /// Activates a specific encryption key version
        /// </summary>
        /// <param name="version">The key version to activate</param>
        /// <param name="purpose">The purpose of the key</param>
        /// <returns>True if activation was successful</returns>
        Task<bool> ActivateKeyVersionAsync(int version, string purpose = "field-encryption");
        
        /// <summary>
        /// Gets all encryption keys for a specific purpose
        /// </summary>
        /// <param name="purpose">The purpose of the keys</param>
        /// <param name="includeExpired">Whether to include expired keys</param>
        /// <returns>List of encryption keys</returns>
        Task<List<EncryptionKey>> GetKeysAsync(string purpose = "field-encryption", bool includeExpired = false);
        
        /// <summary>
        /// Performs scheduled key rotation check and generates new keys if needed
        /// </summary>
        /// <returns>True if a new key was generated</returns>
        Task<bool> PerformScheduledKeyRotationAsync();
    }
}