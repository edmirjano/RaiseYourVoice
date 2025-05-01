using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
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
    }
}