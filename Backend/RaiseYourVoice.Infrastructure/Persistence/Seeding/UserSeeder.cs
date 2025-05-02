using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Seeding
{
    public class UserSeeder : BaseDataSeeder<User>
    {
        private readonly IPasswordHasher _passwordHasher;

        public UserSeeder(MongoDbContext dbContext, ILogger<UserSeeder> logger, IPasswordHasher passwordHasher) 
            : base(dbContext, logger, "Users")
        {
            _passwordHasher = passwordHasher;
        }

        protected override async Task SeedDataAsync()
        {
            var users = new List<User>
            {
                // Admin user
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "System Administrator",
                    Email = "admin@raiseyourvoice.al",
                    PasswordHash = _passwordHasher.HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    ProfilePicture = "https://storage.raiseyourvoice.al/profiles/default-admin.webp",
                    Bio = "System administrator for the RaiseYourVoice platform",
                    JoinDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Preferences = new UserPreferences
                    {
                        PreferredLanguage = "en",
                        NotificationSettings = new NotificationSettings
                        {
                            EnableEmailNotifications = true,
                            EnablePushNotifications = true,
                            EnableActivityDigest = true
                        }
                    },
                    DeviceTokens = new List<UserDevice>()
                },
                
                // Moderator user
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Content Moderator",
                    Email = "moderator@raiseyourvoice.al",
                    PasswordHash = _passwordHasher.HashPassword("Moderator123!"),
                    Role = UserRole.Moderator,
                    ProfilePicture = "https://storage.raiseyourvoice.al/profiles/default-moderator.webp",
                    Bio = "Content moderator for the RaiseYourVoice platform",
                    JoinDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Preferences = new UserPreferences
                    {
                        PreferredLanguage = "en",
                        NotificationSettings = new NotificationSettings
                        {
                            EnableEmailNotifications = true,
                            EnablePushNotifications = true,
                            EnableActivityDigest = true
                        }
                    },
                    DeviceTokens = new List<UserDevice>()
                },
                
                // Demo activist
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Demo Activist",
                    Email = "activist@raiseyourvoice.al",
                    PasswordHash = _passwordHasher.HashPassword("Activist123!"),
                    Role = UserRole.Activist,
                    ProfilePicture = "https://storage.raiseyourvoice.al/profiles/default-activist.webp",
                    Bio = "Passionate activist fighting for environmental causes",
                    JoinDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Preferences = new UserPreferences
                    {
                        PreferredLanguage = "en",
                        NotificationSettings = new NotificationSettings
                        {
                            EnableEmailNotifications = true,
                            EnablePushNotifications = true,
                            EnableActivityDigest = true
                        }
                    },
                    DeviceTokens = new List<UserDevice>()
                }
            };

            await _collection.InsertManyAsync(users);
            _logger.LogInformation("Inserted {Count} default users", users.Count);
        }
    }
}