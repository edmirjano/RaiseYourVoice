using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class CommentRepository : MongoRepository<Comment>
    {
        public CommentRepository(MongoDbContext context, ILogger<CommentRepository> logger) 
            : base(context, "Comments", logger)
        {
        }

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(string postId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(string authorId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.AuthorId, authorId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetRootCommentsByPostIdAsync(string postId)
        {
            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.PostId, postId),
                Builders<Comment>.Filter.Eq(c => c.ParentCommentId, null)
            );
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetRepliesAsync(string commentId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.ParentCommentId, commentId);
            return await _collection.Find(filter).ToListAsync();
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

        public async Task<bool> DecrementLikeCountAsync(string commentId)
        {
            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.Id, commentId),
                Builders<Comment>.Filter.Gt(c => c.LikeCount, 0)
            );
            var update = Builders<Comment>.Update
                .Inc(c => c.LikeCount, -1)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> MarkAsReportedAsync(string commentId, string reportedBy, string reason)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.Id, commentId);
            var update = Builders<Comment>.Update
                .Set(c => c.IsReported, true)
                .Set(c => c.ReportedBy, reportedBy)
                .Set(c => c.ReportReason, reason)
                .Set(c => c.ReportedAt, DateTime.UtcNow)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> ResolveReportAsync(string commentId, bool isApproved, string resolvedBy)
        {
            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.Id, commentId),
                Builders<Comment>.Filter.Eq(c => c.IsReported, true)
            );

            var update = Builders<Comment>.Update
                .Set(c => c.IsReported, false)
                .Set(c => c.ReportResolvedBy, resolvedBy)
                .Set(c => c.ReportResolvedAt, DateTime.UtcNow)
                .Set(c => c.IsHidden, !isApproved)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Comment>> GetPaginatedCommentsAsync(
            string postId,
            int pageNumber,
            int pageSize,
            bool includeReplies = false,
            string? sortBy = null,
            bool ascending = false)
        {
            FilterDefinition<Comment> filter;

            if (includeReplies)
            {
                filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
            }
            else
            {
                filter = Builders<Comment>.Filter.And(
                    Builders<Comment>.Filter.Eq(c => c.PostId, postId),
                    Builders<Comment>.Filter.Eq(c => c.ParentCommentId, null)
                );
            }

            // Create sort definition
            SortDefinition<Comment> sortDefinition;

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Map sortBy string to property expression
                switch (sortBy.ToLower())
                {
                    case "likes":
                        sortDefinition = ascending
                            ? Builders<Comment>.Sort.Ascending(c => c.LikeCount)
                            : Builders<Comment>.Sort.Descending(c => c.LikeCount);
                        break;
                    case "date":
                        sortDefinition = ascending
                            ? Builders<Comment>.Sort.Ascending(c => c.CreatedAt)
                            : Builders<Comment>.Sort.Descending(c => c.CreatedAt);
                        break;
                    default:
                        sortDefinition = Builders<Comment>.Sort.Descending(c => c.CreatedAt);
                        break;
                }
            }
            else
            {
                // Default sort by created date descending (newest first)
                sortDefinition = Builders<Comment>.Sort.Descending(c => c.CreatedAt);
            }

            return await _collection
                .Find(filter)
                .Sort(sortDefinition)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountCommentsAsync(string postId, bool includeReplies = false)
        {
            FilterDefinition<Comment> filter;

            if (includeReplies)
            {
                filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
            }
            else
            {
                filter = Builders<Comment>.Filter.And(
                    Builders<Comment>.Filter.Eq(c => c.PostId, postId),
                    Builders<Comment>.Filter.Eq(c => c.ParentCommentId, null)
                );
            }

            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<IEnumerable<Comment>> GetReportedCommentsAsync()
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.IsReported, true);
            return await _collection.Find(filter).ToListAsync();
        }
    }
}