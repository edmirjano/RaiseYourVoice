using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Domain.Entities
{
    public class LocalizationEntry : BaseEntity
    {
        public string Key { get; set; }
        public string Language { get; set; }
        public string Value { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }
}