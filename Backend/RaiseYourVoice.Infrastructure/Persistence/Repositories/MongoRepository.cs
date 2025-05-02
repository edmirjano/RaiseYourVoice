using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Pagination;
using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class MongoRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly IMongoCollection<T> _collection;
        protected readonly ILogger<MongoRepository<T>> _logger;

        public MongoRepository(MongoDbContext context, string collectionName, ILogger<MongoRepository<T>> logger)
        {
            _collection = context.Database.GetCollection<T>(collectionName);
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _collection.Find(expression).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by ID: {Message}", ex.Message);
                return default;
            }
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            var result = await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(e => e.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public virtual async Task<PagedResult<T>> GetPagedAsync(PaginationParameters parameters)
        {
            return await GetPagedAsync(parameters, _ => true);
        }

        public virtual async Task<PagedResult<T>> GetPagedAsync(PaginationParameters parameters, Expression<Func<T, bool>> filterExpression)
        {
            try
            {
                // Get total count for pagination metadata
                var totalItemsCount = await _collection.CountDocumentsAsync(filterExpression);
                
                // If no items found, return empty result
                if (totalItemsCount == 0)
                {
                    return PagedResult<T>.Empty(parameters.PageNumber, parameters.PageSize);
                }
                
                // Create the query with filtering and pagination
                var query = _collection.Find(filterExpression);
                
                // Apply sorting if specified
                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                {
                    // Map the sort field to the appropriate property in the entity
                    // This will need to be overridden in derived repositories for custom sorting
                    var sortDefinition = GetSortDefinition(parameters.SortBy, parameters.Ascending);
                    if (sortDefinition != null)
                    {
                        query = query.Sort(sortDefinition);
                    }
                }
                else
                {
                    // Default sort by CreatedAt descending if no sort field specified
                    query = query.Sort(Builders<T>.Sort.Descending(e => e.CreatedAt));
                }
                
                // Calculate skip for pagination
                var skip = (parameters.PageNumber - 1) * parameters.PageSize;
                
                // Apply pagination and execute query
                var items = await query
                    .Skip(skip)
                    .Limit(parameters.PageSize)
                    .ToListAsync();
                
                // Return paged result
                return PagedResult<T>.Create(
                    items, 
                    totalItemsCount, 
                    parameters.PageNumber, 
                    parameters.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged results: {Message}", ex.Message);
                return PagedResult<T>.Empty(parameters.PageNumber, parameters.PageSize);
            }
        }

        /// <summary>
        /// Creates a sort definition based on the provided sort field and direction
        /// Can be overridden in derived repositories for entity-specific sorting
        /// </summary>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="ascending">Sort direction</param>
        /// <returns>MongoDB sort definition</returns>
        protected virtual SortDefinition<T>? GetSortDefinition(string sortBy, bool ascending)
        {
            // Default implementation handles common base entity fields
            // Derived repositories should override this for entity-specific fields
            return sortBy?.ToLowerInvariant() switch
            {
                "createdat" => ascending 
                    ? Builders<T>.Sort.Ascending(e => e.CreatedAt) 
                    : Builders<T>.Sort.Descending(e => e.CreatedAt),
                "updatedat" => ascending 
                    ? Builders<T>.Sort.Ascending(e => e.UpdatedAt) 
                    : Builders<T>.Sort.Descending(e => e.UpdatedAt),
                "id" => ascending 
                    ? Builders<T>.Sort.Ascending(e => e.Id) 
                    : Builders<T>.Sort.Descending(e => e.Id),
                _ => null // Default to null if field not recognized
            };
        }
    }
}