using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace RaiseYourVoice.Infrastructure.Persistence.Seeding
{
    public abstract class BaseDataSeeder<T> : IDataSeeder
    {
        protected readonly IMongoCollection<T> _collection;
        protected readonly ILogger<BaseDataSeeder<T>> _logger;

        protected BaseDataSeeder(MongoDbContext dbContext, ILogger<BaseDataSeeder<T>> logger, string collectionName)
        {
            _collection = dbContext.Database.GetCollection<T>(collectionName);
            _logger = logger;
        }

        public async Task<bool> SeedingRequiredAsync()
        {
            try
            {
                var count = await _collection.CountDocumentsAsync(FilterDefinition<T>.Empty);
                return count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if seeding is required for {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }
        
        public async Task SeedAsync()
        {
            try
            {
                if (await SeedingRequiredAsync())
                {
                    _logger.LogInformation("Seeding data for {CollectionName}", _collection.CollectionNamespace.CollectionName);
                    await SeedDataAsync();
                    _logger.LogInformation("Successfully seeded data for {CollectionName}", _collection.CollectionNamespace.CollectionName);
                }
                else
                {
                    _logger.LogInformation("Seeding not required for {CollectionName} as data already exists", _collection.CollectionNamespace.CollectionName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding data for {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public async Task ReseedAsync()
        {
            try
            {
                _logger.LogInformation("Reseeding data for {CollectionName}", _collection.CollectionNamespace.CollectionName);
                await _collection.DeleteManyAsync(FilterDefinition<T>.Empty);
                await SeedDataAsync();
                _logger.LogInformation("Successfully reseeded data for {CollectionName}", _collection.CollectionNamespace.CollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reseeding data for {CollectionName}", _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        /// <summary>
        /// Implement this method to provide the actual data seeding logic
        /// </summary>
        protected abstract Task SeedDataAsync();
    }
}