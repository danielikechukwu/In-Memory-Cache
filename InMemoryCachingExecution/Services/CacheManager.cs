using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Collections.Concurrent;

namespace InMemoryCachingExecution.Services
{
    public class CacheManager
    {
        // We hold an instance of IMemoryCache to perform actual caching operations.
        private readonly IMemoryCache _cache;

        // We use a thread-safe ConcurrentDictionary to track cache keys.
        private readonly ConcurrentDictionary<string, bool> _cacheKeys;

        // The constructor receives an IMemoryCache from DI.
        public CacheManager(IMemoryCache cache)
        {
            _cache = cache;
            _cacheKeys = new ConcurrentDictionary<string, bool>();
        }

        // Adds a cache entry and tracks its key in our ConcurrentDictionary.
        // options can define expiration strategies, priority, etc.
        public void Set<T>(string key, T value, MemoryCacheEntryOptions options)
        {
            // Set the cache with the provided key and value.
            _cache.Set(key, value, options);

            // Track the cache key in our dictionary.
            _cacheKeys.TryAdd(key, true);
        }

        // Attempts to retrieve a cache entry.
        // If the key exists in the IMemoryCache, returns true along with the value.
        // Otherwise, removes it from our dictionary.
        public bool TryGetValue<T>(string key, out T? value)
        {
            if (_cache.TryGetValue(key, out value))
            {
                return true;
            }

            // If not found in the cache, remove from the dictionary
            _cacheKeys.TryRemove(key, out _);

            value = default;

            return false;
        }

        // Removes a cache entry from both IMemoryCache and our dictionary.
        public void Remove(string key)
        {
            // Remove the cache entry from IMemoryCache.
            _cache.Remove(key);

            // Also remove the key from our tracking dictionary.
            _cacheKeys.TryRemove(key, out _);
        }

        // Returns all currently known (tracked) cache keys.
        // Note: This might include keys that recently expired, so you may want to
        // re-check each key in IMemoryCache if you want only actively stored ones.
        public List<string> GetAllKeys()
        {
            return _cacheKeys.Keys.ToList();
        }

        // Clears all cache entries from IMemoryCache and resets our dictionary.
        public void ClearCache()
        {
            foreach (var key in _cacheKeys.Keys)
            {
                _cache.Remove(key);
            }

            _cacheKeys.Clear();
        }
    }
}
