using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RaiseYourVoice.Api.Security;

namespace RaiseYourVoice.Api.Middleware
{
    /// <summary>
    /// Extensions for registering and using the API path encryption middleware
    /// </summary>
    public static class ApiPathEncryptionMiddlewareExtensions
    {
        /// <summary>
        /// Adds API path encryption services to the service collection
        /// </summary>
        public static IServiceCollection AddApiPathEncryption(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure options from configuration
            services.Configure<ApiPathEncryptionOptions>(
                configuration.GetSection("ApiPathEncryptionSettings"));
                
            // Register the helper service
            services.AddScoped<ApiPathEncryptionHelper>();
                
            return services;
        }
        
        /// <summary>
        /// Uses the API path encryption middleware in the request pipeline
        /// </summary>
        public static IApplicationBuilder UseApiPathEncryption(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ApiPathEncryptionMiddleware>();
        }
    }
}