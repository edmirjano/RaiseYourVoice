namespace RaiseYourVoice.Api.Middleware
{
    /// <summary>
    /// Extension methods for adding the request/response logging middleware to the application pipeline
    /// </summary>
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        /// <summary>
        /// Adds middleware to log detailed HTTP request and response information with sensitive data protection
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder for chaining</returns>
        public static IApplicationBuilder UseRequestResponseLogging(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}