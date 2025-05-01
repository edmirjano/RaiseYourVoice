using Microsoft.AspNetCore.Builder;

namespace RaiseYourVoice.Api.Middleware
{
    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyValidation(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}