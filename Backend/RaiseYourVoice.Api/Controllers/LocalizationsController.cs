using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaiseYourVoice.Api.Controllers
{
    public class LocalizationsController : BaseApiController
    {
        public LocalizationsController(ILocalizationService localizationService)
            : base(localizationService)
        {
        }
        
        /// <summary>
        /// Get all translations for the current language
        /// </summary>
        /// <returns>Dictionary of translation keys and their values</returns>
        [HttpGet]
        public async Task<ActionResult<IDictionary<string, string>>> GetAllTranslations()
        {
            var language = GetCurrentLanguage();
            var translations = await _localizationService.GetAllStringsForLanguageAsync(language);
            return Ok(translations);
        }
        
        /// <summary>
        /// Get translations for a specific category and the current language
        /// </summary>
        /// <param name="category">The category to get translations for</param>
        /// <returns>Dictionary of translation keys and their values</returns>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IDictionary<string, string>>> GetTranslationsByCategory(string category)
        {
            var language = GetCurrentLanguage();
            var translations = await _localizationService.GetStringsByCategoryAsync(category, language);
            return Ok(translations);
        }
        
        /// <summary>
        /// Get a single translation by key
        /// </summary>
        /// <param name="key">The translation key</param>
        /// <returns>The translated value</returns>
        [HttpGet("{key}")]
        public ActionResult<string> GetTranslation(string key)
        {
            var language = GetCurrentLanguage();
            var translation = _localizationService.GetLocalizedString(key, language);
            return Ok(new { key, value = translation });
        }
        
        /// <summary>
        /// Add or update a translation
        /// </summary>
        /// <param name="request">Translation data</param>
        /// <returns>Success indicator</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetTranslation([FromBody] TranslationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Value))
            {
                return BadRequest(new { message = "Key and value are required" });
            }
            
            var result = await _localizationService.SetLocalizedStringAsync(
                request.Key,
                request.Language,
                request.Value,
                request.Category,
                request.Description);
                
            if (result)
            {
                return Ok(new { message = "Translation saved successfully" });
            }
            
            return BadRequest(new { message = "Failed to save translation" });
        }
    }
    
    public class TranslationRequest
    {
        public string Key { get; set; }
        public string Language { get; set; }
        public string Value { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }
}