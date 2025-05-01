namespace RaiseYourVoice.Application.Interfaces
{
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets a localized string for the specified key and language
        /// </summary>
        /// <param name="key">The key identifying the string to translate</param>
        /// <param name="language">The language code (e.g., "en", "sq")</param>
        /// <returns>The translated string or the key itself if not found</returns>
        string GetLocalizedString(string key, string language);
        
        /// <summary>
        /// Gets a localized string with format parameters
        /// </summary>
        /// <param name="key">The key identifying the string to translate</param>
        /// <param name="language">The language code (e.g., "en", "sq")</param>
        /// <param name="args">Format arguments to insert into the translated string</param>
        /// <returns>The translated and formatted string or the key itself if not found</returns>
        string GetLocalizedString(string key, string language, params object[] args);
        
        /// <summary>
        /// Retrieves all localized strings for a specific language
        /// </summary>
        /// <param name="language">The language code</param>
        /// <returns>Dictionary mapping keys to translated strings</returns>
        Task<IDictionary<string, string>> GetAllStringsForLanguageAsync(string language);
        
        /// <summary>
        /// Adds or updates a localized string
        /// </summary>
        /// <param name="key">The key identifying the string</param>
        /// <param name="language">The language code</param>
        /// <param name="value">The translated value</param>
        /// <param name="category">Optional category for grouping translations</param>
        /// <param name="description">Optional description providing context for translators</param>
        /// <returns>Success indicator</returns>
        Task<bool> SetLocalizedStringAsync(string key, string language, string value, string? category = null, string? description = null);
        
        /// <summary>
        /// Retrieves translations by category
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <param name="language">The language code</param>
        /// <returns>Dictionary mapping keys to translated strings</returns>
        Task<IDictionary<string, string>> GetStringsByCategoryAsync(string category, string language);
    }
}