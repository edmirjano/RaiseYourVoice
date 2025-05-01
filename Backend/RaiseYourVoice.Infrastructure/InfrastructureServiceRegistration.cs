using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Infrastructure.Persistence;
using RaiseYourVoice.Infrastructure.Persistence.Repositories;
using RaiseYourVoice.Infrastructure.Security;
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
            services.Configure<MongoDbSettings>(options => 
            {
                options.ConnectionString = configuration.GetSection("MongoDbSettings:ConnectionString").Value;
                options.DatabaseName = configuration.GetSection("MongoDbSettings:DatabaseName").Value;
            });
            
            services.AddSingleton<MongoDbContext>();
            services.AddSingleton<IMongoClient>(sp => 
            {
                var settings = new MongoDbSettings
                {
                    ConnectionString = configuration.GetSection("MongoDbSettings:ConnectionString").Value
                };
                return new MongoClient(settings.ConnectionString);
            });

            // Register generic repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(MongoRepository<>));
            
            // Register specific repositories
            services.AddScoped<IGenericRepository<User>, UserRepository>();
            services.AddScoped<IGenericRepository<Post>, PostRepository>();
            services.AddScoped<IGenericRepository<Comment>, CommentRepository>();
            services.AddScoped<IGenericRepository<Organization>, OrganizationRepository>();
            services.AddScoped<IGenericRepository<Campaign>, CampaignRepository>();
            services.AddScoped<IGenericRepository<Donation>, DonationRepository>();
            services.AddScoped<IGenericRepository<Notification>, NotificationRepository>();
            services.AddScoped<IGenericRepository<RefreshToken>, RefreshTokenRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IEncryptionKeyRepository, EncryptionKeyRepository>();

            // Register services
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<IDonationService, DonationService>();
            services.AddScoped<ILocalizationService, LocalizationService>();

            // Register security services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<EncryptionLoggingService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddSingleton<JwtKeyManager>();

            // Configure payment gateway
            services.Configure<StripeSettings>(options =>
            {
                options.SecretKey = configuration.GetSection("Stripe:SecretKey").Value;
                options.PublishableKey = configuration.GetSection("Stripe:PublishableKey").Value;
                options.WebhookSecret = configuration.GetSection("Stripe:WebhookSecret").Value;
            });
            
            services.AddScoped<IPaymentGateway, StripePaymentGateway>();

            // Configure Redis for caching
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
                options.InstanceName = "RYV_";
            });
            
            // Configure key rotation
            services.Configure<KeyRotationOptions>(options =>
            {
                var rotationSection = configuration.GetSection("KeyRotationSettings");
                options.RotationIntervalDays = rotationSection.GetValue<int?>("RotationIntervalDays") ?? 30;
                options.KeyGracePeriodDays = rotationSection.GetValue<int?>("KeyGracePeriodDays") ?? 7;
                options.AutomaticRotation = rotationSection.GetValue<bool>("AutomaticRotation", true);
                options.RotationCheckIntervalHours = rotationSection.GetValue<int?>("RotationCheckIntervalHours") ?? 24;
            });

            services.AddHostedService<KeyRotationBackgroundService>();

            return services;
        }
    }
}