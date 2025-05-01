namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for translation creation or update
    /// </summary>
    public class TranslationRequest
    {
        /// <summary>
        /// The unique key identifying the translation
        /// </summary>
        public string Key { get; set; }
        
        /// <summary>
        /// The language code (e.g., "en", "sq")
        /// </summary>
        public string Language { get; set; }
        
        /// <summary>
        /// The translated value
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// Optional category for organizing translations
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Optional description providing context for translators
        /// </summary>
        public string Description { get; set; }
    }
}