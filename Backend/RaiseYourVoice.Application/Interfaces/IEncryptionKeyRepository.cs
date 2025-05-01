using System.Collections.Generic;
using System.Threading.Tasks;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Application.Interfaces
{
    /// <summary>
    /// Repository interface for managing encryption keys
    /// </summary>
    public interface IEncryptionKeyRepository : IGenericRepository<EncryptionKey>
    {
        /// <summary>
        /// Gets the currently active key for a specific purpose
        /// </summary>
        /// <param name="purpose">The purpose of the key</param>
        /// <returns>The active encryption key</returns>
        Task<EncryptionKey> GetActiveKeyAsync(string purpose);
        
        /// <summary>
        /// Gets a key by its version and purpose
        /// </summary>
        /// <param name="version">The key version</param>
        /// <param name="purpose">The purpose of the key</param>
        /// <returns>The encryption key</returns>
        Task<EncryptionKey> GetKeyByVersionAsync(int version, string purpose);
        
        /// <summary>
        /// Gets all keys for a specific purpose
        /// </summary>
        /// <param name="purpose">The purpose of the keys</param>
        /// <param name="includeExpired">Whether to include expired keys</param>
        /// <returns>List of encryption keys</returns>
        Task<List<EncryptionKey>> GetKeysByPurposeAsync(string purpose, bool includeExpired = false);
        
        /// <summary>
        /// Activates a specific key version and deactivates all other keys for the same purpose
        /// </summary>
        /// <param name="keyId">The ID of the key to activate</param>
        /// <param name="purpose">The purpose of the key</param>
        /// <returns>True if activation was successful</returns>
        Task<bool> ActivateKeyAsync(string keyId, string purpose);
        
        /// <summary>
        /// Gets the highest version number for a specific purpose
        /// </summary>
        /// <param name="purpose">The purpose of the keys</param>
        /// <returns>The highest version number</returns>
        Task<int> GetHighestVersionAsync(string purpose);
    }
}