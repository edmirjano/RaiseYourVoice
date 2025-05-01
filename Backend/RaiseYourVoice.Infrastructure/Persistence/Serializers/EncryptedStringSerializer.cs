using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using RaiseYourVoice.Application.Interfaces;

namespace RaiseYourVoice.Infrastructure.Persistence.Serializers
{
    /// <summary>
    /// MongoDB serializer that automatically encrypts and decrypts string properties marked with [Encrypted]
    /// </summary>
    public class EncryptedStringSerializer : SerializerBase<string>
    {
        private readonly IEncryptionService _encryptionService;
        private readonly bool _isEncryptedField;

        public EncryptedStringSerializer(IEncryptionService encryptionService, bool isEncryptedField)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _isEncryptedField = isEncryptedField;
        }

        public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Get the value from MongoDB
            var value = context.Reader.ReadString();

            // If it's not an encrypted field or empty, return as is
            if (!_isEncryptedField || string.IsNullOrEmpty(value))
            {
                return value;
            }

            try
            {
                // Decrypt the value
                return _encryptionService.Decrypt(value);
            }
            catch (Exception ex)
            {
                // Log the exception (ideally using a proper logging framework)
                Console.WriteLine($"Error decrypting field: {ex.Message}");
                
                // Return a placeholder for UI indication of decryption failure
                return "[DECRYPTION ERROR]";
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
        {
            if (!_isEncryptedField || string.IsNullOrEmpty(value))
            {
                // Write the value as is if it's not marked for encryption or is empty
                context.Writer.WriteString(value);
                return;
            }

            try
            {
                // Encrypt the value before storing
                var encrypted = _encryptionService.Encrypt(value);
                context.Writer.WriteString(encrypted);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error encrypting field: {ex.Message}");
                
                // Write the original value as fallback (could also throw an exception based on security policy)
                context.Writer.WriteString(value);
            }
        }
    }
}