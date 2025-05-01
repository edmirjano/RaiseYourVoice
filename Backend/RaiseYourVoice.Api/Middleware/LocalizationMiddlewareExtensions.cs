using Microsoft.AspNetCore.Builder;

namespace RaiseYourVoice.Api.Middleware
{
    public static class LocalizationMiddlewareExtensions
    {
        /// <summary>
        /// Adds the localization middleware to the application pipeline
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseLocalization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LocalizationMiddleware>();
        }
    }
}