using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class PostRepository : MongoRepository<Post>
    {
        public PostRepository(MongoDbContext context, ILogger<PostRepository> logger) 
            : base(context, "Posts", logger)
        {
        }

        public async Task<IEnumerable<Post>> GetByAuthorIdAsync(string authorId)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.AuthorId, authorId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByPostTypeAsync(PostType postType)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.PostType, postType);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByStatusAsync(PostStatus status)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Status, status);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPublishedPostsAsync()
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Status, PostStatus.Published);
            return await _collection.Find(filter)
                .Sort(Builders<Post>.Sort.Descending(p => p.CreatedAt))
                .ToListAsync();
        }

        public async Task<bool> IncrementViewCountAsync(string postId)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
            var update = Builders<Post>.Update.Inc(p => p.ViewCount, 1);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
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

        public async Task<bool> DecrementLikeCountAsync(string postId)
        {
            var filter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.Eq(p => p.Id, postId),
                Builders<Post>.Filter.Gt(p => p.LikeCount, 0)
            );
            var update = Builders<Post>.Update
                .Inc(p => p.LikeCount, -1)
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
            var filter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.Eq(p => p.Id, postId),
                Builders<Post>.Filter.Gt(p => p.CommentCount, 0)
            );
            var update = Builders<Post>.Update
                .Inc(p => p.CommentCount, -1)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdatePostStatusAsync(string postId, PostStatus status)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
            var update = Builders<Post>.Update
                .Set(p => p.Status, status)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Post>> GetPaginatedPostsAsync(
            int pageNumber,
            int pageSize,
            PostType? postType = null,
            PostStatus? status = null,
            string? authorId = null,
            string? sortBy = null,
            bool ascending = false)
        {
            var filterBuilder = Builders<Post>.Filter;
            var filters = new List<FilterDefinition<Post>>();

            // Apply filters
            if (postType.HasValue)
            {
                filters.Add(filterBuilder.Eq(p => p.PostType, postType.Value));
            }

            if (status.HasValue)
            {
                filters.Add(filterBuilder.Eq(p => p.Status, status.Value));
            }

            if (!string.IsNullOrEmpty(authorId))
            {
                filters.Add(filterBuilder.Eq(p => p.AuthorId, authorId));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Create sort definition
            SortDefinition<Post> sortDefinition;

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Map sortBy string to property expression
                switch (sortBy.ToLower())
                {
                    case "likes":
                        sortDefinition = ascending
                            ? Builders<Post>.Sort.Ascending(p => p.LikeCount)
                            : Builders<Post>.Sort.Descending(p => p.LikeCount);
                        break;
                    case "comments":
                        sortDefinition = ascending
                            ? Builders<Post>.Sort.Ascending(p => p.CommentCount)
                            : Builders<Post>.Sort.Descending(p => p.CommentCount);
                        break;
                    case "views":
                        sortDefinition = ascending
                            ? Builders<Post>.Sort.Ascending(p => p.ViewCount)
                            : Builders<Post>.Sort.Descending(p => p.ViewCount);
                        break;
                    case "date":
                        sortDefinition = ascending
                            ? Builders<Post>.Sort.Ascending(p => p.CreatedAt)
                            : Builders<Post>.Sort.Descending(p => p.CreatedAt);
                        break;
                    default:
                        sortDefinition = Builders<Post>.Sort.Descending(p => p.CreatedAt);
                        break;
                }
            }
            else
            {
                // Default sort by created date descending (newest first)
                sortDefinition = Builders<Post>.Sort.Descending(p => p.CreatedAt);
            }

            return await _collection
                .Find(combinedFilter)
                .Sort(sortDefinition)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountPostsAsync(
            PostType? postType = null,
            PostStatus? status = null,
            string? authorId = null)
        {
            var filterBuilder = Builders<Post>.Filter;
            var filters = new List<FilterDefinition<Post>>();

            // Apply filters
            if (postType.HasValue)
            {
                filters.Add(filterBuilder.Eq(p => p.PostType, postType.Value));
            }

            if (status.HasValue)
            {
                filters.Add(filterBuilder.Eq(p => p.Status, status.Value));
            }

            if (!string.IsNullOrEmpty(authorId))
            {
                filters.Add(filterBuilder.Eq(p => p.AuthorId, authorId));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return await _collection.CountDocumentsAsync(combinedFilter);
        }

        public async Task<IEnumerable<Post>> SearchPostsAsync(
            string searchText,
            PostType? postType = null,
            PostStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var filterBuilder = Builders<Post>.Filter;
            var filters = new List<FilterDefinition<Post>>();

            // Text search filter
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(p => p.Title, new MongoDB.Bson.BsonRegularExpression(searchText, "i")),
                filterBuilder.Regex(p => p.Content, new MongoDB.Bson.BsonRegularExpression(searchText, "i"))
            );
            filters.Add(searchFilter);

            // Apply additional filters
            if (postType.HasValue)
            {
                filters.Add(filterBuilder.Eq(p => p.PostType, postType.Value));
            }

            if (status.HasValue)
            {
                filters.Add(filterBuilder.Eq(p => p.Status, status.Value));
            }

            if (fromDate.HasValue)
            {
                filters.Add(filterBuilder.Gte(p => p.CreatedAt, fromDate.Value));
            }

            if (toDate.HasValue)
            {
                filters.Add(filterBuilder.Lte(p => p.CreatedAt, toDate.Value));
            }

            var combinedFilter = filterBuilder.And(filters);

            return await _collection
                .Find(combinedFilter)
                .Sort(Builders<Post>.Sort.Descending(p => p.CreatedAt))
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetTrendingPostsAsync(int limit = 10)
        {
            // Calculate trending score based on likes, comments and views with recency factor
            var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
            
            // Posts from the last two weeks get higher weight
            var filter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.Eq(p => p.Status, PostStatus.Published),
                Builders<Post>.Filter.Gte(p => p.CreatedAt, twoWeeksAgo)
            );

            // Custom score based on likes (weight: 2), comments (weight: 1.5), views (weight: 0.5)
            // This creates a pipeline to sort by a computed "trending score"
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "Status", PostStatus.Published.ToString() },
                    { "CreatedAt", new BsonDocument("$gte", twoWeeksAgo) }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "trendingScore", new BsonDocument("$add", new BsonArray
                        {
                            new BsonDocument("$multiply", new BsonArray { "$LikeCount", 2 }),
                            new BsonDocument("$multiply", new BsonArray { "$CommentCount", 1.5 }),
                            new BsonDocument("$multiply", new BsonArray { "$ViewCount", 0.5 })
                        })
                    }
                }),
                new BsonDocument("$sort", new BsonDocument("trendingScore", -1)),
                new BsonDocument("$limit", limit)
            };

            // Execute aggregation pipeline
            return await _collection.Aggregate<Post>(pipeline).ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByTagsAsync(IEnumerable<string> tags)
        {
            var filter = Builders<Post>.Filter.AnyIn(p => p.Tags, tags);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<string>> GetPopularTagsAsync(int limit = 10)
        {
            // Aggregate to find most used tags across all posts
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "Status", PostStatus.Published.ToString() }
                }),
                new BsonDocument("$unwind", "$Tags"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$Tags" },
                    { "count", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("count", -1)),
                new BsonDocument("$limit", limit),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "tag", "$_id" }
                })
            };

            var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(r => r["tag"].AsString);
        }
    }
}