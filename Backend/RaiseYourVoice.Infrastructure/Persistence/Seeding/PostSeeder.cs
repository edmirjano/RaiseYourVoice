using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Seeding
{
    public class PostSeeder : BaseDataSeeder<Post>
    {
        private readonly IMongoCollection<User> _userCollection;
        
        public PostSeeder(MongoDbContext dbContext, ILogger<PostSeeder> logger) 
            : base(dbContext, logger, "Posts")
        {
            _userCollection = dbContext.Users;
        }

        protected override async Task SeedDataAsync()
        {
            // Get users to associate as authors
            var users = await _userCollection.Find(Builders<User>.Filter.Empty)
                .Project(u => new { u.Id, u.Name })
                .ToListAsync();
                
            if (users.Count == 0)
            {
                _logger.LogWarning("No users found for associating with posts. Please seed users first.");
                return;
            }
            
            var posts = new List<Post>();
            var random = new Random();
            
            // Create different types of posts
            posts.AddRange(CreateActivismPosts(users, random));
            posts.AddRange(CreateOpportunityPosts(users, random));
            posts.AddRange(CreateSuccessStoryPosts(users, random));
            
            await _collection.InsertManyAsync(posts);
            _logger.LogInformation("Inserted {Count} sample posts", posts.Count);
        }
        
        private List<Post> CreateActivismPosts(List<dynamic> users, Random random)
        {
            var posts = new List<Post>();
            
            var activismPostTitles = new[]
            {
                "Join the Climate March this Saturday!",
                "Petition for Plastic Bag Ban in Tirana",
                "Environmental Workshop Series Starting Next Week",
                "Student-Led Initiative for School Recycling Programs",
                "Volunteer Opportunity: Beach Cleanup in Durres"
            };
            
            var activismPostContents = new[]
            {
                "We're organizing a climate march this Saturday at 10:00 AM, starting at Skanderbeg Square. Bring your friends, family, and signs! Let's make our voices heard for climate action now. #ClimateActionNow #TiranaForFuture",
                
                "Our petition to ban single-use plastic bags in Tirana has reached 5,000 signatures! We need 10,000 to present it to the city council. Sign and share the petition link in the comments. Together we can reduce plastic pollution in our beautiful city. #PlasticFreeTirana #EnvironmentalAction",
                
                "Excited to announce our new environmental workshop series starting next week! Topics include home composting, sustainable gardening, and reducing your carbon footprint. Free registration at the link below. Limited spots available! #EnvironmentalEducation #SustainableLiving",
                
                "Students at Sami Frasheri High School have started an amazing school-wide recycling program that's already diverted 500kg of waste from landfill! They're creating a toolkit to help other schools implement similar programs. #YouthActivism #RecyclingHeroes",
                
                "Volunteer for our beach cleanup in Durres this Sunday! We'll provide gloves, bags, and refreshments. Just bring your enthusiasm and a friend! Meeting point is the main beach entrance at 9:00 AM. #BeachCleanup #ProtectOurCoast"
            };
            
            for (int i = 0; i < activismPostTitles.Length; i++)
            {
                // Select a random user as the author
                var author = users[random.Next(users.Count)];
                
                posts.Add(new Post
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = activismPostTitles[i],
                    Content = activismPostContents[i],
                    AuthorId = author.Id,
                    PostType = PostType.Activism,
                    Status = PostStatus.Published,
                    MediaUrls = new List<string> { $"https://storage.raiseyourvoice.al/posts/activism-{i + 1}.webp" },
                    Tags = GenerateTagsForActivism(),
                    LikeCount = random.Next(10, 200),
                    CommentCount = random.Next(5, 50),
                    ViewCount = random.Next(100, 1000),
                    Location = new GeoLocation
                    {
                        Type = "Point",
                        Coordinates = new double[] { 19.82 + random.NextDouble() * 0.1, 41.32 + random.NextDouble() * 0.1 }
                    },
                    EventDate = i % 2 == 0 ? DateTime.UtcNow.AddDays(random.Next(1, 14)) : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 5))
                });
            }
            
            return posts;
        }
        
        private List<Post> CreateOpportunityPosts(List<dynamic> users, Random random)
        {
            var posts = new List<Post>();
            
            var opportunityPostTitles = new[]
            {
                "Youth Exchange Program: 2 Weeks in Berlin",
                "Funding Available for Environmental Projects",
                "Internship Opportunity at the EU Delegation",
                "Call for MUN Delegates - Apply by June 10",
                "Summer Camp Volunteers Needed"
            };
            
            var opportunityPostContents = new[]
            {
                "Exciting youth exchange opportunity in Berlin focusing on digital media and activism. The program covers travel, accommodation, and meals. Open to young people aged 18-25. Application deadline: June 15. #YouthMobility #EramsusPlus",
                
                "The Environmental Action Fund is offering grants of up to €5,000 for community-based environmental projects. Priority given to youth-led initiatives. Application form and guidelines available at the link below. #Funding #EnvironmentalProjects",
                
                "The EU Delegation in Tirana is offering 6-month paid internships for recent graduates interested in EU affairs, project management, and communication. Great opportunity to gain professional experience! #EUCareers #Internship",
                
                "The Tirana International Model United Nations is looking for delegates for their upcoming conference. This year's theme is 'Climate Action and Global Cooperation.' No previous MUN experience required. Training provided. #MUN #YouthLeadership",
                
                "Volunteers needed for our summer environmental camp for children! Help us inspire the next generation of environmental leaders. Volunteering periods from 1-4 weeks available throughout July and August. #VolunteerOpportunity #EnvironmentalEducation"
            };
            
            for (int i = 0; i < opportunityPostTitles.Length; i++)
            {
                // Select a random user as the author
                var author = users[random.Next(users.Count)];
                
                posts.Add(new Post
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = opportunityPostTitles[i],
                    Content = opportunityPostContents[i],
                    AuthorId = author.Id,
                    PostType = PostType.Opportunity,
                    Status = PostStatus.Published,
                    MediaUrls = new List<string> { $"https://storage.raiseyourvoice.al/posts/opportunity-{i + 1}.webp" },
                    Tags = GenerateTagsForOpportunity(),
                    LikeCount = random.Next(10, 150),
                    CommentCount = random.Next(5, 40),
                    ViewCount = random.Next(100, 800),
                    Location = new GeoLocation
                    {
                        Type = "Point",
                        Coordinates = new double[] { 19.82 + random.NextDouble() * 0.1, 41.32 + random.NextDouble() * 0.1 }
                    },
                    EventDate = DateTime.UtcNow.AddDays(random.Next(5, 60)),
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 14)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 3))
                });
            }
            
            return posts;
        }
        
        private List<Post> CreateSuccessStoryPosts(List<dynamic> users, Random random)
        {
            var posts = new List<Post>();
            
            var successStoryTitles = new[]
            {
                "How Our Community Garden Transformed the Neighborhood",
                "From School Project to National Policy: The Plastic Ban Journey",
                "Meet Arta: Activist of the Month",
                "Local Youth-Led Campaign Saved the City Park",
                "How a Small Village Became Waste-Free in Just One Year"
            };
            
            var successStoryContents = new[]
            {
                "Three years ago, our neighborhood didn't have any green spaces. Today, our community garden produces fresh vegetables for 50 families, hosts educational workshops, and has become a vibrant community hub. Here's how we did it... #CommunityGarden #UrbanGreenspaces",
                
                "What started as a high school project to reduce plastic waste in our school cafeteria grew into a nationwide movement. After three years of advocacy, data collection, and persistence, we're proud that the national plastic bag ban takes effect next month! #PolicyChange #PlasticPollution",
                
                "We're thrilled to introduce Arta Kelmendi as our Activist of the Month! At just 19, Arta has organized tree-planting initiatives across 12 cities, created an environmental education program reaching 5,000 students, and advised the Ministry of Environment on youth engagement strategies. #ActivistSpotlight #YouthLeadership",
                
                "When developers planned to cut down the old trees in our city park, nobody thought we could stop them. But a dedicated group of young people launched an effective campaign combining legal action, community mobilization, and creative protests. Today, the park has protected status! #SavedByActivism #UrbanNature",
                
                "The village of Prrenjas has achieved something remarkable: reducing their waste to almost zero in just one year. Through community education, composting programs, and innovative reuse systems, they've shown that small communities can make a big environmental impact. #ZeroWaste #SustainableCommunities"
            };
            
            for (int i = 0; i < successStoryTitles.Length; i++)
            {
                // Select a random user as the author
                var author = users[random.Next(users.Count)];
                
                posts.Add(new Post
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = successStoryTitles[i],
                    Content = successStoryContents[i],
                    AuthorId = author.Id,
                    PostType = PostType.SuccessStory,
                    Status = PostStatus.Published,
                    MediaUrls = new List<string> { $"https://storage.raiseyourvoice.al/posts/success-story-{i + 1}.webp" },
                    Tags = GenerateTagsForSuccessStory(),
                    LikeCount = random.Next(50, 300),
                    CommentCount = random.Next(10, 80),
                    ViewCount = random.Next(200, 1500),
                    Location = new GeoLocation
                    {
                        Type = "Point",
                        Coordinates = new double[] { 19.82 + random.NextDouble() * 0.1, 41.32 + random.NextDouble() * 0.1 }
                    },
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(10, 60)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 10))
                });
            }
            
            return posts;
        }
        
        private List<string> GenerateTagsForActivism()
        {
            var allTags = new[] {
                "climateaction", "environmentalism", "sustainability", "activism", 
                "volunteers", "conservation", "zerowaste", "climateemergency",
                "youth", "community", "ecofriendly", "plasticfree", "tirana"
            };
            
            return SelectRandomTags(allTags, 3, 5);
        }
        
        private List<string> GenerateTagsForOpportunity()
        {
            var allTags = new[] {
                "opportunity", "funding", "youthexchange", "internship", "volunteer",
                "education", "career", "erasmusplus", "mobility", "training",
                "workshop", "jobs", "fellowship", "scholarship", "funding"
            };
            
            return SelectRandomTags(allTags, 3, 5);
        }
        
        private List<string> GenerateTagsForSuccessStory()
        {
            var allTags = new[] {
                "success", "inspiration", "impact", "achievement", "changemaker",
                "community", "transformation", "progress", "advocacy", "leadership",
                "localheroes", "grassroots", "youthleadership", "innovation", "breakthrough"
            };
            
            return SelectRandomTags(allTags, 3, 5);
        }
        
        private List<string> SelectRandomTags(string[] allTags, int min, int max)
        {
            var random = new Random();
            var count = random.Next(min, max + 1);
            var selectedIndices = new HashSet<int>();
            var selectedTags = new List<string>();
            
            while (selectedTags.Count < count && selectedTags.Count < allTags.Length)
            {
                var index = random.Next(allTags.Length);
                if (selectedIndices.Add(index))
                {
                    selectedTags.Add(allTags[index]);
                }
            }
            
            return selectedTags;
        }
    }
}