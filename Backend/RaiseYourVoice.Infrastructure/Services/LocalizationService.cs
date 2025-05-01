using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Infrastructure.Persistence;
using System.Text.Json;

namespace RaiseYourVoice.Infrastructure.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IMongoCollection<LocalizationEntry> _localizationCollection;
        private readonly IDistributedCache _cache;
        private readonly Dictionary<string, Dictionary<string, string>> _memoryCache;
        private readonly TimeSpan _cacheExpirationTime = TimeSpan.FromHours(1);
        
        public LocalizationService(
            MongoDbContext dbContext,
            IDistributedCache cache)
        {
            _localizationCollection = dbContext.Database.GetCollection<LocalizationEntry>("Localizations");
            _cache = cache;
            _memoryCache = new Dictionary<string, Dictionary<string, string>>();
            
            // Create indexes if they don't exist
            CreateIndexesAsync().GetAwaiter().GetResult();
        }
        
        private async Task CreateIndexesAsync()
        {
            // Create compound index on Key and Language for fast lookups
            var indexKeysDefinition = Builders<LocalizationEntry>.IndexKeys
                .Ascending(le => le.Key)
                .Ascending(le => le.Language);
            
            var indexOptions = new CreateIndexOptions { Name = "Key_Language_Index", Unique = true };
            await _localizationCollection.Indexes.CreateOneAsync(new CreateIndexModel<LocalizationEntry>(indexKeysDefinition, indexOptions));
            
            // Create index for category lookups
            await _localizationCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<LocalizationEntry>(
                    Builders<LocalizationEntry>.IndexKeys.Ascending(le => le.Category),
                    new CreateIndexOptions { Name = "Category_Index" }
                )
            );
            
            // Create index for language lookups
            await _localizationCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<LocalizationEntry>(
                    Builders<LocalizationEntry>.IndexKeys.Ascending(le => le.Language),
                    new CreateIndexOptions { Name = "Language_Index" }
                )
            );
        }

        public string GetLocalizedString(string key, string language)
        {
            // If no key or language is provided, return the key
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(language))
                return key;
            
            // Default language fallback if the specified language isn't supported
            if (language != "en" && language != "sq")
                language = "en";
                
            // Try in-memory cache first for performance
            if (_memoryCache.TryGetValue(language, out var languageDict) && 
                languageDict.TryGetValue(key, out var translation))
            {
                return translation;
            }
            
            // Then try distributed cache
            string cacheKey = $"loc_{language}_{key}";
            string cachedValue = _cache.GetString(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedValue))
                return cachedValue;
            
            // If not in cache, get from database
            var entry = _localizationCollection.Find(l => l.Key == key && l.Language == language)
                .FirstOrDefault();
                
            // If not found in the specified language, try English as fallback
            if (entry == null && language != "en")
            {
                entry = _localizationCollection.Find(l => l.Key == key && l.Language == "en")
                    .FirstOrDefault();
            }
            
            string value = entry?.Value ?? key;
            
            // Add to distributed cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpirationTime
            };
            _cache.SetString(cacheKey, value, cacheOptions);
            
            // Add to in-memory cache
            if (!_memoryCache.ContainsKey(language))
            {
                _memoryCache[language] = new Dictionary<string, string>();
            }
            _memoryCache[language][key] = value;
            
            return value;
        }

        public string GetLocalizedString(string key, string language, params object[] args)
        {
            string format = GetLocalizedString(key, language);
            
            // If no arguments or format is null, return the format itself
            if (args == null || args.Length == 0 || format == null)
                return format;
                
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                // In case of format error, return the unformatted string
                return format;
            }
        }

        public async Task<IDictionary<string, string>> GetAllStringsForLanguageAsync(string language)
        {
            // If the language isn't supported, fall back to English
            if (language != "en" && language != "sq")
                language = "en";
                
            // Check distributed cache first
            string cacheKey = $"loc_all_{language}";
            string cachedJson = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, string>>(cachedJson);
                }
                catch
                {
                    // If deserialization fails, ignore cache and continue
                }
            }
            
            // If not in cache, get from database
            var entries = await _localizationCollection.Find(l => l.Language == language)
                .ToListAsync();
                
            var result = entries.ToDictionary(e => e.Key, e => e.Value);
            
            // Cache the result
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpirationTime
            };
            
            await _cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(result), 
                cacheOptions
            );
            
            return result;
        }

        public async Task<bool> SetLocalizedStringAsync(string key, string language, string value, string category = null, string description = null)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(language))
                return false;
            
            // Only support English and Albanian
            if (language != "en" && language != "sq")
                return false;
                
            // Upsert the entry (create or update)
            var filter = Builders<LocalizationEntry>.Filter.And(
                Builders<LocalizationEntry>.Filter.Eq(l => l.Key, key),
                Builders<LocalizationEntry>.Filter.Eq(l => l.Language, language)
            );
            
            var update = Builders<LocalizationEntry>.Update
                .Set(l => l.Value, value)
                .Set(l => l.UpdatedAt, DateTime.UtcNow);
                
            if (!string.IsNullOrWhiteSpace(category))
            {
                update = update.Set(l => l.Category, category);
            }
            
            if (!string.IsNullOrWhiteSpace(description))
            {
                update = update.Set(l => l.Description, description);
            }
            
            var options = new FindOneAndUpdateOptions<LocalizationEntry>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            
            var result = await _localizationCollection.FindOneAndUpdateAsync(filter, update, options);
            
            if (result != null)
            {
                // Update caches
                string cacheKey = $"loc_{language}_{key}";
                await _cache.RemoveAsync(cacheKey);
                await _cache.RemoveAsync($"loc_all_{language}");
                
                // Also update the category cache if applicable
                if (!string.IsNullOrWhiteSpace(category))
                {
                    await _cache.RemoveAsync($"loc_cat_{language}_{category}");
                }
                
                // Update memory cache if it exists
                if (_memoryCache.TryGetValue(language, out var languageDict))
                {
                    languageDict[key] = value;
                }
                
                return true;
            }
            
            return false;
        }

        public async Task<IDictionary<string, string>> GetStringsByCategoryAsync(string category, string language)
        {
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(language))
                return new Dictionary<string, string>();
                
            // If the language isn't supported, fall back to English
            if (language != "en" && language != "sq")
                language = "en";
                
            // Check cache first
            string cacheKey = $"loc_cat_{language}_{category}";
            string cachedJson = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, string>>(cachedJson);
                }
                catch
                {
                    // If deserialization fails, ignore cache and continue
                }
            }
            
            // If not in cache, get from database
            var filter = Builders<LocalizationEntry>.Filter.And(
                Builders<LocalizationEntry>.Filter.Eq(l => l.Language, language),
                Builders<LocalizationEntry>.Filter.Eq(l => l.Category, category)
            );
            
            var entries = await _localizationCollection.Find(filter).ToListAsync();
            var result = entries.ToDictionary(e => e.Key, e => e.Value);
            
            // Cache the result
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpirationTime
            };
            
            await _cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(result), 
                cacheOptions
            );
            
            return result;
        }
    }
}