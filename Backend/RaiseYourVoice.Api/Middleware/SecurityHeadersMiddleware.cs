using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

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
            // Security Headers based on OWASP recommendations
            
            // Protect against XSS attacks
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            
            // Prevent MIME type sniffing
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            
            // Protect against clickjacking
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            
            // Content Security Policy
            var csp = "default-src 'self'; " +
                     "img-src 'self' data: https:; " +
                     "font-src 'self'; " +
                     "style-src 'self' 'unsafe-inline'; " +
                     "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                     "connect-src 'self'; " +
                     "frame-ancestors 'none'; " +
                     "upgrade-insecure-requests;";
            
            context.Response.Headers.Add("Content-Security-Policy", csp);
            
            // Referrer Policy
            context.Response.Headers.Add("Referrer-Policy", "no-referrer-when-downgrade");
            
            // HTTP Strict Transport Security (max-age=1 year)
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            
            // Feature Policy
            context.Response.Headers.Add("Permissions-Policy", 
                "camera=(), microphone=(), geolocation=(), payment=()");

            await _next(context);
        }
    }
}