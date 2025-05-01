using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Domain.Entities
{
    public class LocalizationEntry : BaseEntity
    {
        public required string Key { get; set; }
        public required string Language { get; set; }
        public required string Value { get; set; }
        public required string Category { get; set; }
        public required string Description { get; set; }
    }
}