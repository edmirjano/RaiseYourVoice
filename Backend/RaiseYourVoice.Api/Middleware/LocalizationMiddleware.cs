using System.Globalization;

namespace RaiseYourVoice.Api.Middleware
{
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string DefaultLanguage = "en";
        private const string LanguageHeaderKey = "Accept-Language";
        private const string PreferredLanguageKey = "PreferredLanguage";
        
        public LocalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            // Extract language from header or use default
            string language = DefaultLanguage;
            
            // Check if language is specified in header
            if (context.Request.Headers.TryGetValue(LanguageHeaderKey, out var values))
            {
                var headerValue = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(headerValue))
                {
                    // Parse language code from header (e.g. "en-US,en;q=0.9")
                    // Take just the first language code mentioned
                    var languageCode = headerValue.Split(',', ';')[0].Trim();
                    
                    // Extract just the language part (e.g. "en" from "en-US")
                    languageCode = languageCode.Split('-')[0].ToLowerInvariant();
                    
                    // Only allow supported languages (English and Albanian)
                    if (languageCode == "en" || languageCode == "sq")
                    {
                        language = languageCode;
                    }
                }
            }
            
            // Store the language in HttpContext.Items for the current request
            context.Items[PreferredLanguageKey] = language;
            
            // Set the current culture for the request thread
            var culture = new CultureInfo(language);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            
            // Continue with the request pipeline
            await _next(context);
        }
    }
}