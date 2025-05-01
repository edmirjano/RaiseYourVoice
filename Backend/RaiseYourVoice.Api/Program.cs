using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaiseYourVoice.Infrastructure;
using System.Text;
using RaiseYourVoice.Api.Middleware;
using RaiseYourVoice.Infrastructure.Services.Security;
using RaiseYourVoice.Infrastructure.Persistence;
using RaiseYourVoice.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Configure environment-based configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables("RYV_") // Add environment variables with RYV_ prefix for Kubernetes
    .AddUserSecrets<Program>(optional: true); // Use user secrets for local development

// Add services to the container
builder.Services.AddControllers();

// Configure strongly typed settings objects
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RaiseYourVoice API", Version = "v1" });
    
    // Configure Swagger to use JWT authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Add API Key definition for Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key Authentication",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            },
            new string[] {}
        }
    });
});

// Add Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add API Path Encryption services
builder.Services.AddApiPathEncryption(builder.Configuration);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication - now using JwtKeyManager
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    var keyManager = serviceProvider.GetRequiredService<JwtKeyManager>();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKeys = keyManager.GetAllSigningKeys(), // Use all keys from the key manager
        ClockSkew = TimeSpan.Zero // Recommended for refresh token scenarios to avoid overlap
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                // Add a custom header to indicate token expiration
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        // This event is called when tokens are received - we refresh the token validation parameters
        // to ensure we're using the latest keys for validation
        OnMessageReceived = context =>
        {
            var updatedKeyManager = context.HttpContext.RequestServices.GetRequiredService<JwtKeyManager>();
            options.TokenValidationParameters.IssuerSigningKeys = updatedKeyManager.GetAllSigningKeys();
            return Task.CompletedTask;
        }
    };
});

// Add API rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = Microsoft.AspNetCore.RateLimiting.PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(httpContext =>
        Microsoft.AspNetCore.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: _ => new Microsoft.AspNetCore.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = Convert.ToInt32(builder.Configuration["SecuritySettings:ApiRateLimitPerMinute"] ?? "100"),
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = Microsoft.AspNetCore.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddMongoDb(builder.Configuration["MongoDbSettings:ConnectionString"] 
        ?? throw new InvalidOperationException("MongoDB connection string is not configured."))
    .AddRedis(builder.Configuration.GetConnectionString("RedisConnection") 
        ?? throw new InvalidOperationException("Redis connection string is not configured."));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global error handling middleware (should be first in the pipeline)
app.UseErrorHandling();

app.UseHttpsRedirection();

// Add security headers middleware
app.UseSecurityHeaders();

// Add localization middleware
app.UseLocalization();

// Add API path encryption middleware
app.UseApiPathEncryption();

// Add API key validation middleware
app.UseApiKeyValidation();

app.UseRateLimiter();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Log configuration source (but not values) for debugging
if (app.Environment.IsDevelopment())
{
    var configurationSources = ((IConfigurationRoot)app.Configuration).Providers.Select(p => p.GetType().Name);
    app.Logger.LogInformation("Configuration providers: {Providers}", string.Join(", ", configurationSources));
}

// Create MongoDB indexes during startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await dbContext.EnsureIndexesAsync();
    app.Logger.LogInformation("MongoDB indexes created successfully");
}

app.Run();
