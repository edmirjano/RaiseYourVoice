using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Domain.Entities
{
    /// <summary>
    /// Represents an encryption key with versioning for key rotation purposes
    /// </summary>
    public class EncryptionKey : BaseEntity
    {
        /// <summary>
        /// Unique key identifier (version)
        /// </summary>
        public int Version { get; set; }
        
        /// <summary>
        /// The encryption key material (Base64-encoded)
        /// </summary>
        public required string Key { get; set; }
        
        /// <summary>
        /// The initialization vector for this key (Base64-encoded)
        /// </summary>
        public required string IV { get; set; }
        
        /// <summary>
        /// When the key was created (renamed to avoid hiding base member)
        /// </summary>
        public DateTime KeyCreatedAt { get; set; }
        
        /// <summary>
        /// When the key becomes active
        /// </summary>
        public DateTime ActivatedAt { get; set; }
        
        /// <summary>
        /// When the key expires (should no longer be used for encryption)
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Whether this is the current active key for encryption
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The purpose of this key (e.g., "field-encryption", "api-path")
        /// </summary>
        public required string Purpose { get; set; }
        
        /// <summary>
        /// Optional description or metadata
        /// </summary>
        public string? Description { get; set; }
    }
}