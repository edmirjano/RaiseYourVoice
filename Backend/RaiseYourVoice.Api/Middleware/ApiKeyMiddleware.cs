namespace RaiseYourVoice.Api.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-API-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the endpoint requires API key validation
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<RequireApiKeyAttribute>() == null)
            {
                await _next(context);
                return;
            }

            // Check for API key in the request header
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKeys = appSettings.GetSection("SecuritySettings:ApiKeys").Get<string[]>();

            // Validate API key
            if (apiKeys == null || !Array.Exists(apiKeys, key => key == extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            await _next(context);
        }
    }

    // Attribute to mark controllers or actions that require API key authentication
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireApiKeyAttribute : Attribute
    {
    }
}