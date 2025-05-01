using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Domain.Entities;
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

            // Register services
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<IDonationService, DonationService>();
            services.AddScoped<ILocalizationService, LocalizationService>();

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