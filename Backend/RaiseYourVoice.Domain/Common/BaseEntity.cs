using MongoDB.Bson;

namespace RaiseYourVoice.Domain.Common
{
    public abstract class BaseEntity
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}