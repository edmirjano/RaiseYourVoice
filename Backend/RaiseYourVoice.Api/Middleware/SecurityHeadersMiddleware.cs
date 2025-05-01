using Microsoft.AspNetCore.Http;

namespace RaiseYourVoice.Api.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Security Headers
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            
            // HSTS - HTTP Strict Transport Security
            // Only add HSTS header if request is over HTTPS
            if (context.Request.IsHttps)
            {
                // maxage = 1 year in seconds, include subdomains, preload
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            }
            
            // Content Security Policy
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "img-src 'self' https: data:; " +
                "font-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "connect-src 'self' https:; " +
                "frame-ancestors 'none'; " +
                "upgrade-insecure-requests;";
                
            // Permissions Policy
            context.Response.Headers["Permissions-Policy"] = 
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
                "magnetometer=(), microphone=(), payment=(), usb=()";
                
            // Referrer Policy
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            
            // Cache Control
            // Not included for dynamic API responses, as this is handled per endpoint
            
            await _next(context);
        }
    }
}