using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using RaiseYourVoice.Infrastructure.Persistence;
using System.Linq;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class PostRepository : MongoGenericRepository<Post>
    {
        public PostRepository(MongoDbContext context) : base(context, "Posts")
        {
        }

        public async Task<IEnumerable<Post>> GetTrendingPostsAsync(int limit = 10)
        {
            return await _collection.Find(_ => true)
                .SortByDescending(p => p.LikeCount + p.CommentCount)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByAuthorIdAsync(string authorId)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.AuthorId, authorId);
            return await _collection.Find(filter)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByTypeAsync(PostType postType, int limit = 20, int skip = 0)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.PostType, postType);
            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IncrementLikeCountAsync(string postId)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
            var update = Builders<Post>.Update
                .Inc(p => p.LikeCount, 1)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> IncrementCommentCountAsync(string postId)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
            var update = Builders<Post>.Update
                .Inc(p => p.CommentCount, 1)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DecrementCommentCountAsync(string postId)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
            var update = Builders<Post>.Update
                .Inc(p => p.CommentCount, -1)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Post>> SearchPostsAsync(string searchTerm, int limit = 20)
        {
            var filter = Builders<Post>.Filter.Or(
                Builders<Post>.Filter.Regex(p => p.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Post>.Filter.Regex(p => p.Content, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            
            return await _collection.Find(filter)
                .SortByDescending(p => p.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByLocationAsync(double latitude, double longitude, double maxDistanceInKm = 10)
        {
            // Convert km to radians (Earth's radius is approximately 6371 km)
            double maxDistanceInRadians = maxDistanceInKm / 6371.0;
            
            // Create a geospatial query to find posts near the given coordinates
            var filter = Builders<Post>.Filter.NearSphere(
                p => p.Location, 
                longitude, 
                latitude, 
                maxDistanceInRadians
            );
            
            return await _collection.Find(filter)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}