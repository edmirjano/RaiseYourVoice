using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Infrastructure.Persistence.Conventions;

namespace RaiseYourVoice.Infrastructure.Persistence
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IEncryptionService _encryptionService;

        public MongoDbContext(IOptions<MongoDbSettings> settings, IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
            
            // Register encryption convention
            RegisterConventions();
            
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        private void RegisterConventions()
        {
            // Create a convention pack
            var conventionPack = new ConventionPack
            {
                new EncryptedFieldsConvention(_encryptionService)
            };

            // Register the convention pack
            ConventionRegistry.Register("EncryptedFieldsConvention", conventionPack, t => true);
        }

        public IMongoDatabase Database => _database;

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Post> Posts => _database.GetCollection<Post>("Posts");
        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("Comments");
        public IMongoCollection<Organization> Organizations => _database.GetCollection<Organization>("Organizations");
        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");
        public IMongoCollection<Campaign> Campaigns => _database.GetCollection<Campaign>("Campaigns");
        public IMongoCollection<Donation> Donations => _database.GetCollection<Donation>("Donations");
        public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("RefreshTokens");
        public IMongoCollection<EncryptionKey> EncryptionKeys => _database.GetCollection<EncryptionKey>("EncryptionKeys");

        /// <summary>
        /// Creates indexes for all collections to optimize query performance
        /// </summary>
        public async Task EnsureIndexesAsync()
        {
            // User indexes
            await Users.Indexes.CreateOneAsync(
                new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.Email),
                    new CreateIndexOptions { Unique = true, Name = "Email_Unique" }
                )
            );
            
            await Users.Indexes.CreateOneAsync(
                new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.Role),
                    new CreateIndexOptions { Name = "Role_Index" }
                )
            );

            // Post indexes
            await Posts.Indexes.CreateOneAsync(
                new CreateIndexModel<Post>(
                    Builders<Post>.IndexKeys.Ascending(p => p.AuthorId),
                    new CreateIndexOptions { Name = "AuthorId_Index" }
                )
            );
            
            await Posts.Indexes.CreateOneAsync(
                new CreateIndexModel<Post>(
                    Builders<Post>.IndexKeys.Descending(p => p.CreatedAt),
                    new CreateIndexOptions { Name = "CreatedAt_Index" }
                )
            );
            
            await Posts.Indexes.CreateOneAsync(
                new CreateIndexModel<Post>(
                    Builders<Post>.IndexKeys.Ascending(p => p.PostType),
                    new CreateIndexOptions { Name = "PostType_Index" }
                )
            );
            
            await Posts.Indexes.CreateOneAsync(
                new CreateIndexModel<Post>(
                    Builders<Post>.IndexKeys.Descending(p => p.LikeCount).Descending(p => p.CommentCount),
                    new CreateIndexOptions { Name = "Engagement_Index" }
                )
            );
            
            await Posts.Indexes.CreateOneAsync(
                new CreateIndexModel<Post>(
                    Builders<Post>.IndexKeys.Geo2DSphere(p => p.Location),
                    new CreateIndexOptions { Name = "Location_Geo_Index" }
                )
            );

            // Comment indexes
            await Comments.Indexes.CreateOneAsync(
                new CreateIndexModel<Comment>(
                    Builders<Comment>.IndexKeys.Ascending(c => c.PostId),
                    new CreateIndexOptions { Name = "PostId_Index" }
                )
            );
            
            await Comments.Indexes.CreateOneAsync(
                new CreateIndexModel<Comment>(
                    Builders<Comment>.IndexKeys.Ascending(c => c.AuthorId),
                    new CreateIndexOptions { Name = "AuthorId_Index" }
                )
            );
            
            await Comments.Indexes.CreateOneAsync(
                new CreateIndexModel<Comment>(
                    Builders<Comment>.IndexKeys.Ascending(c => c.ParentCommentId),
                    new CreateIndexOptions { Name = "ParentComment_Index" }
                )
            );

            // Organization indexes
            await Organizations.Indexes.CreateOneAsync(
                new CreateIndexModel<Organization>(
                    Builders<Organization>.IndexKeys.Ascending(o => o.VerificationStatus),
                    new CreateIndexOptions { Name = "VerificationStatus_Index" }
                )
            );
            
            await Organizations.Indexes.CreateOneAsync(
                new CreateIndexModel<Organization>(
                    Builders<Organization>.IndexKeys.Ascending(o => o.OrganizationType),
                    new CreateIndexOptions { Name = "OrganizationType_Index" }
                )
            );
            
            await Organizations.Indexes.CreateOneAsync(
                new CreateIndexModel<Organization>(
                    Builders<Organization>.IndexKeys.Text(o => o.Name).Text(o => o.Description),
                    new CreateIndexOptions { Name = "Text_Search_Index" }
                )
            );
            
            await Organizations.Indexes.CreateOneAsync(
                new CreateIndexModel<Organization>(
                    Builders<Organization>.IndexKeys.Geo2DSphere(o => o.HeadquartersLocation),
                    new CreateIndexOptions { Name = "HQ_Location_Geo_Index" }
                )
            );

            // Campaign indexes
            await Campaigns.Indexes.CreateOneAsync(
                new CreateIndexModel<Campaign>(
                    Builders<Campaign>.IndexKeys.Ascending(c => c.OrganizationId),
                    new CreateIndexOptions { Name = "OrganizationId_Index" }
                )
            );
            
            await Campaigns.Indexes.CreateOneAsync(
                new CreateIndexModel<Campaign>(
                    Builders<Campaign>.IndexKeys.Ascending(c => c.Status),
                    new CreateIndexOptions { Name = "Status_Index" }
                )
            );
            
            await Campaigns.Indexes.CreateOneAsync(
                new CreateIndexModel<Campaign>(
                    Builders<Campaign>.IndexKeys.Ascending(c => c.IsFeatured),
                    new CreateIndexOptions { Name = "Featured_Index" }
                )
            );
            
            await Campaigns.Indexes.CreateOneAsync(
                new CreateIndexModel<Campaign>(
                    Builders<Campaign>.IndexKeys.Ascending(c => c.StartDate).Ascending(c => c.EndDate),
                    new CreateIndexOptions { Name = "DateRange_Index" }
                )
            );

            // Donation indexes
            await Donations.Indexes.CreateOneAsync(
                new CreateIndexModel<Donation>(
                    Builders<Donation>.IndexKeys.Ascending(d => d.CampaignId),
                    new CreateIndexOptions { Name = "CampaignId_Index" }
                )
            );
            
            await Donations.Indexes.CreateOneAsync(
                new CreateIndexModel<Donation>(
                    Builders<Donation>.IndexKeys.Ascending(d => d.DonorId),
                    new CreateIndexOptions { Name = "DonorId_Index" }
                )
            );
            
            await Donations.Indexes.CreateOneAsync(
                new CreateIndexModel<Donation>(
                    Builders<Donation>.IndexKeys.Ascending(d => d.Status),
                    new CreateIndexOptions { Name = "Status_Index" }
                )
            );

            // Notification indexes
            await Notifications.Indexes.CreateOneAsync(
                new CreateIndexModel<Notification>(
                    Builders<Notification>.IndexKeys.Ascending("RecipientIds"),
                    new CreateIndexOptions { Name = "Recipients_Index" }
                )
            );
            
            await Notifications.Indexes.CreateOneAsync(
                new CreateIndexModel<Notification>(
                    Builders<Notification>.IndexKeys.Ascending(n => n.Type),
                    new CreateIndexOptions { Name = "Type_Index" }
                )
            );
            
            await Notifications.Indexes.CreateOneAsync(
                new CreateIndexModel<Notification>(
                    Builders<Notification>.IndexKeys.Descending(n => n.SentAt),
                    new CreateIndexOptions { Name = "SentAt_Index" }
                )
            );
            
            await Notifications.Indexes.CreateOneAsync(
                new CreateIndexModel<Notification>(
                    Builders<Notification>.IndexKeys.Ascending(n => n.ExpiresAt),
                    new CreateIndexOptions { Name = "Expiry_Index" }
                )
            );

            // RefreshToken indexes
            await RefreshTokens.Indexes.CreateOneAsync(
                new CreateIndexModel<RefreshToken>(
                    Builders<RefreshToken>.IndexKeys.Ascending(r => r.Token),
                    new CreateIndexOptions { Unique = true, Name = "Token_Unique" }
                )
            );
            
            await RefreshTokens.Indexes.CreateOneAsync(
                new CreateIndexModel<RefreshToken>(
                    Builders<RefreshToken>.IndexKeys.Ascending(r => r.UserId),
                    new CreateIndexOptions { Name = "UserId_Index" }
                )
            );
            
            await RefreshTokens.Indexes.CreateOneAsync(
                new CreateIndexModel<RefreshToken>(
                    Builders<RefreshToken>.IndexKeys.Ascending(r => r.ExpiresAt),
                    new CreateIndexOptions { Name = "Expiry_Index" }
                )
            );

            // EncryptionKey indexes
            await EncryptionKeys.Indexes.CreateOneAsync(
                new CreateIndexModel<EncryptionKey>(
                    Builders<EncryptionKey>.IndexKeys.Ascending(k => k.Purpose).Ascending(k => k.Version),
                    new CreateIndexOptions { Name = "Purpose_Version_Index", Unique = true }
                )
            );
            
            await EncryptionKeys.Indexes.CreateOneAsync(
                new CreateIndexModel<EncryptionKey>(
                    Builders<EncryptionKey>.IndexKeys.Ascending(k => k.Purpose).Descending(k => k.IsActive),
                    new CreateIndexOptions { Name = "Purpose_Active_Index" }
                )
            );
            
            await EncryptionKeys.Indexes.CreateOneAsync(
                new CreateIndexModel<EncryptionKey>(
                    Builders<EncryptionKey>.IndexKeys.Ascending(k => k.ExpiresAt),
                    new CreateIndexOptions { Name = "ExpiresAt_Index" }
                )
            );
        }
    }

    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}