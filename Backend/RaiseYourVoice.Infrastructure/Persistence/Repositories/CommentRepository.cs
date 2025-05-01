using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class CommentRepository : MongoGenericRepository<Comment>
    {
        public CommentRepository(MongoDbContext context) : base(context, "Comments")
        {
        }

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(string postId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
            return await _collection.Find(filter)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetRootCommentsByPostIdAsync(string postId, int limit = 20, int skip = 0)
        {
            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.PostId, postId),
                Builders<Comment>.Filter.Eq(c => c.ParentCommentId, null)
            );
            
            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetRepliesByCommentIdAsync(string parentCommentId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.ParentCommentId, parentCommentId);
            return await _collection.Find(filter)
                .SortBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(string authorId, int limit = 20, int skip = 0)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.AuthorId, authorId);
            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IncrementLikeCountAsync(string commentId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.Id, commentId);
            var update = Builders<Comment>.Update
                .Inc(c => c.LikeCount, 1)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<int> CountByPostIdAsync(string postId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<IEnumerable<Comment>> GetAllThreadedByPostIdAsync(string postId)
        {
            // Get all comments for a post in a single query, then we can organize them in-memory
            var filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
            var allComments = await _collection.Find(filter)
                .SortBy(c => c.CreatedAt)
                .ToListAsync();
                
            return allComments;
        }

        public async Task<bool> DeleteAllByPostIdAsync(string postId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}