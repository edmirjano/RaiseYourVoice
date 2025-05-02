using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Seeding
{
    public class CommentSeeder : BaseDataSeeder<Comment>
    {
        private readonly IMongoCollection<Post> _postCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly PostRepository _postRepository;
        
        public CommentSeeder(
            MongoDbContext dbContext, 
            ILogger<CommentSeeder> logger,
            PostRepository postRepository) 
            : base(dbContext, logger, "Comments")
        {
            _postCollection = dbContext.Posts;
            _userCollection = dbContext.Users;
            _postRepository = postRepository;
        }

        protected override async Task SeedDataAsync()
        {
            // Get posts to comment on
            var posts = await _postCollection.Find(Builders<Post>.Filter.Empty)
                .Project(p => new { p.Id })
                .ToListAsync();
                
            if (posts.Count == 0)
            {
                _logger.LogWarning("No posts found for adding comments. Please seed posts first.");
                return;
            }
            
            // Get users to associate as commenters
            var users = await _userCollection.Find(Builders<User>.Filter.Empty)
                .Project(u => new { u.Id, u.Name })
                .ToListAsync();
                
            if (users.Count == 0)
            {
                _logger.LogWarning("No users found for associating with comments. Please seed users first.");
                return;
            }
            
            var comments = new List<Comment>();
            var random = new Random();
            
            // For each post, create several comments
            foreach (var post in posts)
            {
                int commentCount = random.Next(3, 10); // Random number of comments per post
                var rootComments = new List<string>(); // To track comments that can have replies
                
                for (int i = 0; i < commentCount; i++)
                {
                    // Select a random user as the commenter
                    var user = users[random.Next(users.Count)];
                    
                    // Create a comment
                    var comment = new Comment
                    {
                        Id = Guid.NewGuid().ToString(),
                        PostId = post.Id,
                        AuthorId = user.Id,
                        Content = GetRandomComment(),
                        ParentCommentId = i > 0 && random.Next(100) < 30 ? rootComments[random.Next(rootComments.Count)] : null, // 30% chance of being a reply
                        LikeCount = random.Next(0, 30),
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 20)),
                        UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 3)),
                        IsReported = false,
                        IsHidden = false
                    };
                    
                    if (comment.ParentCommentId == null)
                    {
                        rootComments.Add(comment.Id);
                    }
                    
                    comments.Add(comment);
                }
                
                // Update the post's comment count
                await _postRepository.UpdatePostStatusAsync(post.Id, RaiseYourVoice.Domain.Enums.PostStatus.Published);
            }
            
            await _collection.InsertManyAsync(comments);
            
            // Update comment counts for posts
            foreach (var post in posts)
            {
                var count = comments.Count(c => c.PostId == post.Id && c.ParentCommentId == null);
                if (count > 0)
                {
                    await _postRepository.UpdatePostStatusAsync(post.Id, RaiseYourVoice.Domain.Enums.PostStatus.Published);
                }
            }
            
            _logger.LogInformation("Inserted {Count} sample comments", comments.Count);
        }
        
        private string GetRandomComment()
        {
            var comments = new[]
            {
                "This is such an important initiative! I fully support it.",
                "Great work! I'd love to get involved. How can I help?",
                "This is exactly what our community needs right now.",
                "I've been thinking about this issue for a while. So glad someone is taking action!",
                "Thanks for organizing this. Count me in for the next event!",
                "Really inspiring to see young people making such a difference.",
                "I participated in a similar project last year. The impact was incredible.",
                "Would love to see this expanded to other areas as well.",
                "Just shared this with my friends. Everyone should know about this.",
                "This is the kind of positive news we need more of!",
                "Do you have any resources for someone who wants to learn more about this?",
                "I think this approach could really work. Looking forward to seeing the results.",
                "This reminds me of a similar initiative in another city that was very successful.",
                "I've been following this issue for years. So happy to see progress finally happening.",
                "Very well written and informative post. Thank you for sharing.",
                "This is exactly the kind of opportunity I've been looking for!",
                "When is the next meeting? I'd like to join.",
                "Have you considered partnering with local schools on this?",
                "This could be a game-changer for our community.",
                "I'm skeptical about how sustainable this is in the long term. What's the plan for continued funding?",
                "Amazing project! The photos really show the impact you're making.",
                "I love seeing real solutions instead of just talk. Great job!",
                "This deserves more attention. I'll help spread the word.",
                "As someone who works in this field, I can confirm this approach makes a lot of sense.",
                "I wish more people would take initiative like this!"
            };
            
            return comments[new Random().Next(comments.Length)];
        }
    }
}