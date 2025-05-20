using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Application.Models.Responses;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using RaiseYourVoice.Infrastructure.Persistence;
using Testcontainers.MongoDb;
using Testcontainers.Redis;

namespace RaiseYourVoice.IntegrationTests;

public class TestBase : IAsyncLifetime
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly MongoDbContainer _mongoDbContainer;
    protected readonly RedisContainer _redisContainer;
    protected readonly JsonSerializerOptions _jsonOptions;

    public TestBase()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:latest")
            .WithPortBinding(27017, true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:alpine")
            .WithPortBinding(6379, true)
            .Build();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.Test.json");
                    
                    // Override with container connection strings
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["MongoDbSettings:ConnectionString"] = _mongoDbContainer.GetConnectionString(),
                        ["ConnectionStrings:RedisConnection"] = _redisContainer.GetConnectionString()
                    });
                });
                
                builder.ConfigureTestServices(services =>
                {
                    // Add any test-specific service overrides here
                    // For example, you might want to use in-memory services instead of real ones
                    
                    // Configure logging to be less verbose during tests
                    services.Configure<ILoggingBuilder>(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    });
                });
            });

        _client = _factory.CreateClient();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();
        await _redisContainer.StartAsync();
        
        // Seed the database with test data
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _mongoDbContainer.StopAsync();
        await _redisContainer.StopAsync();
        _factory.Dispose();
    }

    protected async Task SeedTestDataAsync()
    {
        // Get the MongoDB context from the service provider
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        
        // Clear existing data
        await ClearDatabaseAsync(dbContext);
        
        // Seed users
        await SeedUsersAsync(dbContext);
        
        // Seed organizations
        await SeedOrganizationsAsync(dbContext);
        
        // Seed campaigns
        await SeedCampaignsAsync(dbContext);
        
        // Seed posts
        await SeedPostsAsync(dbContext);
        
        // Seed comments
        await SeedCommentsAsync(dbContext);
    }

    private async Task ClearDatabaseAsync(MongoDbContext dbContext)
    {
        // Drop all collections
        var collections = await dbContext.Database.ListCollectionNamesAsync();
        var collectionNames = await collections.ToListAsync();
        
        foreach (var collectionName in collectionNames)
        {
            await dbContext.Database.DropCollectionAsync(collectionName);
        }
    }

    private async Task SeedUsersAsync(MongoDbContext dbContext)
    {
        var users = new List<User>
        {
            new User
            {
                Id = "user1",
                Name = "Admin User",
                Email = "admin@test.com",
                PasswordHash = "$2a$11$ov4c0OMZFEIJqxs.bk6LLOdwdcMrDIUqXLQYsKJNNK1bXqJtXL7om", // Password: Admin123!
                Role = UserRole.Admin,
                JoinDate = DateTime.UtcNow.AddDays(-30),
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
                Preferences = new UserPreferences
                {
                    PreferredLanguage = "en",
                    NotificationSettings = new NotificationSettings()
                }
            },
            new User
            {
                Id = "user2",
                Name = "Regular User",
                Email = "user@test.com",
                PasswordHash = "$2a$11$ov4c0OMZFEIJqxs.bk6LLOdwdcMrDIUqXLQYsKJNNK1bXqJtXL7om", // Password: Admin123!
                Role = UserRole.Activist,
                JoinDate = DateTime.UtcNow.AddDays(-15),
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow,
                Preferences = new UserPreferences
                {
                    PreferredLanguage = "en",
                    NotificationSettings = new NotificationSettings()
                }
            },
            new User
            {
                Id = "user3",
                Name = "Organization User",
                Email = "org@test.com",
                PasswordHash = "$2a$11$ov4c0OMZFEIJqxs.bk6LLOdwdcMrDIUqXLQYsKJNNK1bXqJtXL7om", // Password: Admin123!
                Role = UserRole.Organization,
                JoinDate = DateTime.UtcNow.AddDays(-20),
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow,
                Preferences = new UserPreferences
                {
                    PreferredLanguage = "en",
                    NotificationSettings = new NotificationSettings()
                }
            }
        };
        
        await dbContext.Users.InsertManyAsync(users);
    }

    private async Task SeedOrganizationsAsync(MongoDbContext dbContext)
    {
        var organizations = new List<Organization>
        {
            new Organization
            {
                Id = "org1",
                Name = "Test Organization",
                Description = "A test organization for integration tests",
                VerificationStatus = VerificationStatus.Verified,
                VerifiedBy = "user1",
                VerificationDate = DateTime.UtcNow.AddDays(-10),
                FoundingDate = DateTime.UtcNow.AddYears(-2),
                OrganizationType = OrganizationType.NGO,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };
        
        await dbContext.Organizations.InsertManyAsync(organizations);
    }

    private async Task SeedCampaignsAsync(MongoDbContext dbContext)
    {
        var campaigns = new List<Campaign>
        {
            new Campaign
            {
                Id = "campaign1",
                Title = "Test Campaign",
                Description = "A test campaign for integration tests",
                OrganizationId = "org1",
                Goal = 10000,
                AmountRaised = 5000,
                StartDate = DateTime.UtcNow.AddDays(-20),
                EndDate = DateTime.UtcNow.AddDays(10),
                Status = CampaignStatus.Active,
                Category = CampaignCategory.Education,
                IsFeatured = true,
                ViewCount = 100,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };
        
        await dbContext.Campaigns.InsertManyAsync(campaigns);
    }

    private async Task SeedPostsAsync(MongoDbContext dbContext)
    {
        var posts = new List<Post>
        {
            new Post
            {
                Id = "post1",
                Title = "Test Post",
                Content = "This is a test post for integration tests",
                PostType = PostType.Activism,
                AuthorId = "user2",
                LikeCount = 10,
                CommentCount = 2,
                Status = PostStatus.Published,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };
        
        await dbContext.Posts.InsertManyAsync(posts);
    }

    private async Task SeedCommentsAsync(MongoDbContext dbContext)
    {
        var comments = new List<Comment>
        {
            new Comment
            {
                Id = "comment1",
                PostId = "post1",
                AuthorId = "user2",
                Content = "This is a test comment",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Comment
            {
                Id = "comment2",
                PostId = "post1",
                AuthorId = "user3",
                Content = "This is another test comment",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };
        
        await dbContext.Comments.InsertManyAsync(comments);
    }

    protected async Task<string> GetAuthTokenAsync(string email = "admin@test.com", string password = "Admin123!")
    {
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseString, _jsonOptions);
        
        return authResponse?.Token ?? string.Empty;
    }

    protected HttpClient GetAuthenticatedClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}