using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Infrastructure.Persistence
{
    public class MongoGenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly IMongoCollection<T> _collection;

        public MongoGenericRepository(MongoDbContext context, string collectionName)
        {
            _collection = context.Database.GetCollection<T>(collectionName);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _collection.Find(expression).ToListAsync();
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(doc => doc.Id, id);
            return await _collection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = entity.CreatedAt;
            
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            
            var filter = Builders<T>.Filter.Eq(doc => doc.Id, entity.Id);
            var result = await _collection.ReplaceOneAsync(filter, entity);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(doc => doc.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}