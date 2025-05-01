using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Infrastructure.Persistence;
using RaiseYourVoice.Infrastructure.Persistence.Repositories;
using RaiseYourVoice.Infrastructure.Services;
using RaiseYourVoice.Infrastructure.Services.Security;
using MongoDB.Driver;

namespace RaiseYourVoice.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure MongoDB
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
            services.AddSingleton<MongoDbContext>();
            services.AddSingleton<IMongoClient>(sp => 
            {
                var settings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
                return new MongoClient(settings.ConnectionString);
            });

            // Register repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(MongoRepository<>));

            // Register services
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<IDonationService, DonationService>();

            // Register security services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Configure payment gateway
            services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
            services.AddScoped<IPaymentGateway, StripePaymentGateway>();

            // Configure Redis for caching
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
                options.InstanceName = "RYV_";
            });

            return services;
        }
    }
}