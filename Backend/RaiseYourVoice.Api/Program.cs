using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaiseYourVoice.Infrastructure;
using System.Text;
using RaiseYourVoice.Api.Middleware;
using RaiseYourVoice.Infrastructure.Services.Security;
using RaiseYourVoice.Infrastructure.Persistence;
using RaiseYourVoice.Infrastructure.Security;
using RaiseYourVoice.Infrastructure.Persistence.Seeding;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using RaiseYourVoice.Api.gRPC;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using RaiseYourVoice.Api.Swagger;

// Initialize Serilog logger before anything else
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting RaiseYourVoice API");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog with settings from configuration
    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            new CompactJsonFormatter(),
            Path.Combine("Logs", "ryv-api-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30)
        .WriteTo.MongoDB(
            context.Configuration.GetConnectionString("MongoDbConnection") ?? throw new InvalidOperationException("MongoDB connection string is not configured."),
            collectionName: "Logs",
            restrictedToMinimumLevel: LogEventLevel.Information));

    // Configure environment-based configuration
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables("RYV_") // Add environment variables with RYV_ prefix for Kubernetes
        .AddUserSecrets<Program>(optional: true); // Use user secrets for local development

    // Add services to the container
    builder.Services.AddControllers();

    // Add API versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Configure strongly typed settings objects
    var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
    builder.Services.Configure<JwtSettings>(jwtSettingsSection);

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    builder.Services.AddSwaggerGen();

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
        // Configure JWT Bearer token validation - we'll get the actual keys from the key manager when needed
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ClockSkew = TimeSpan.Zero // Recommended for refresh token scenarios to avoid overlap
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var keyManager = context.HttpContext.RequestServices.GetRequiredService<JwtKeyManager>();
                options.TokenValidationParameters.IssuerSigningKeys = keyManager.GetAllSigningKeys();
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    // Add a custom header to indicate token expiration
                    context.Response.Headers.Append("Token-Expired", "true");
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
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        
        options.AddFixedWindowLimiter("fixed", options =>
        {
            options.AutoReplenishment = true;
            options.PermitLimit = Convert.ToInt32(builder.Configuration["SecuritySettings:ApiRateLimitPerMinute"] ?? "100");
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueLimit = 0; // No queuing, just reject when limit is hit
        });
    });

    // Configure Health Checks
    builder.Services.AddHealthChecks()
        .AddMongoDb(
            connectionString: builder.Configuration.GetConnectionString("MongoDbConnection") ?? throw new InvalidOperationException("MongoDB connection string is not configured."),
            name: "mongodb",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "db", "mongodb", "data" })
        .AddRedis(
            redisConnectionString: builder.Configuration.GetConnectionString("RedisConnection") ?? throw new InvalidOperationException("Redis connection string is not configured."),
            name: "redis",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "cache", "redis" });

    // Add health checks UI
    builder.Services.AddHealthChecksUI(options =>
    {
        options.SetEvaluationTimeInSeconds(300); // Evaluate health every 5 minutes
        options.MaximumHistoryEntriesPerEndpoint(50); // Keep history of 50 checks per endpoint
        options.SetApiMaxActiveRequests(1); // Run health checks sequentially
    }).AddInMemoryStorage();

    // Add gRPC services
    builder.Services.AddGrpc(options =>
    {
        options.EnableDetailedErrors = true;
        options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16 MB
        options.MaxSendMessageSize = 16 * 1024 * 1024; // 16 MB
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline with Serilog
    app.UseSerilogRequestLogging(options =>
    {
        // Customize the message template
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        
        // Attach additional properties
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
            
            // Add user identity information if authenticated
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
                diagnosticContext.Set("UserRoles", string.Join(",", 
                    httpContext.User.Claims
                        .Where(c => c.Type == "Role" || c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Value)));
            }
        };
    });

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            // Build a swagger endpoint for each discovered API version
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });
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
    
    // Map gRPC services
    app.MapGrpcService<PostServiceImpl>();
    app.MapGrpcService<AuthServiceImpl>();
    app.MapGrpcService<CommentServiceImpl>();

    // Map health checks with detailed responses
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Map health checks UI
    app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

    // Log configuration source (but not values) for debugging
    if (app.Environment.IsDevelopment())
    {
        var configurationSources = ((IConfigurationRoot)app.Configuration).Providers.Select(p => p.GetType().Name);
        Log.Information("Configuration providers: {Providers}", string.Join(", ", configurationSources));
    }

    // Create MongoDB indexes during startup
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        await dbContext.EnsureIndexesAsync();
        Log.Information("MongoDB indexes created successfully");
        
        // Seed initial data if in development environment or if SEED_DATA environment variable is set
        bool seedData = app.Environment.IsDevelopment() || 
                        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SEED_DATA"));
                        
        if (seedData)
        {
            try
            {
                Log.Information("Seeding initial data...");
                var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeederCoordinator>();
                await dataSeeder.SeedAllAsync();
                Log.Information("Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while seeding the database");
            }
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}