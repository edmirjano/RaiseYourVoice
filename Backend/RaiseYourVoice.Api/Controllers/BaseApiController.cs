using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private const string PreferredLanguageKey = "PreferredLanguage";
        protected readonly ILocalizationService _localizationService;
        
        public BaseApiController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }
        
        /// <summary>
        /// Gets the current user's preferred language from the request context
        /// </summary>
        /// <returns>The language code (e.g., "en", "sq")</returns>
        protected string GetCurrentLanguage()
        {
            return HttpContext.Items[PreferredLanguageKey] as string ?? "en";
        }
        
        /// <summary>
        /// Gets a localized string for the current language
        /// </summary>
        /// <param name="key">The translation key</param>
        /// <returns>The translated string</returns>
        protected string L(string key)
        {
            return _localizationService.GetLocalizedString(key, GetCurrentLanguage());
        }
        
        /// <summary>
        /// Gets a localized string with format parameters for the current language
        /// </summary>
        /// <param name="key">The translation key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>The translated and formatted string</returns>
        protected string L(string key, params object[] args)
        {
            return _localizationService.GetLocalizedString(key, GetCurrentLanguage(), args);
        }
        
        /// <summary>
        /// Creates a success response with a localized message
        /// </summary>
        /// <param name="messageKey">The translation key for the message</param>
        /// <param name="data">Optional data to include in the response</param>
        /// <returns>An action result with the message and data</returns>
        protected IActionResult SuccessWithLocalizedMessage(string messageKey, object data = null)
        {
            var response = new
            {
                success = true,
                message = L(messageKey),
                data
            };
            
            return Ok(response);
        }
        
        /// <summary>
        /// Creates an error response with a localized message
        /// </summary>
        /// <param name="messageKey">The translation key for the error message</param>
        /// <param name="statusCode">The HTTP status code</param>
        /// <returns>An action result with the error message</returns>
        protected IActionResult ErrorWithLocalizedMessage(string messageKey, int statusCode = 400)
        {
            var response = new
            {
                success = false,
                message = L(messageKey)
            };
            
            return StatusCode(statusCode, response);
        }
    }
}