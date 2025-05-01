using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Infrastructure.Persistence.Serializers;
using System.Reflection;

namespace RaiseYourVoice.Infrastructure.Persistence.Conventions
{
    /// <summary>
    /// MongoDB convention that automatically applies encryption to properties marked with [Encrypted]
    /// </summary>
    public class EncryptedFieldsConvention : ConventionBase, IMemberMapConvention
    {
        private readonly IEncryptionService _encryptionService;

        public EncryptedFieldsConvention(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public void Apply(BsonMemberMap memberMap)
        {
            // Check if the property has the Encrypted attribute
            bool isEncrypted = memberMap.MemberInfo.GetCustomAttribute<EncryptedAttribute>() != null;

            // If the property is marked for encryption and is a string type
            if (isEncrypted && memberMap.MemberType == typeof(string))
            {
                // Apply our custom serializer for this field
                memberMap.SetSerializer(new EncryptedStringSerializer(_encryptionService, true));
            }
        }
    }
}