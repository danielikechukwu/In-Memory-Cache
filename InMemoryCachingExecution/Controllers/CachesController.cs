using InMemoryCachingExecution.Data;
using InMemoryCachingExecution.Repository;
using InMemoryCachingExecution.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCachingExecution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CachesController : ControllerBase
    {
        private readonly ILogger<CachesController> _logger;

        // Service to manage cache keys and operations.
        private readonly CacheManager _cacheManager;

        private readonly IMemoryCache _memoryCache;

        // Contructor that accepts ILogger, CacheManager, and IMemoryCache instances. For dependency injection.
        public CachesController(ILogger<CachesController> logger, CacheManager cacheManager, IMemoryCache memoryCache)
        {
            _logger = logger;
            _cacheManager = cacheManager;
            _memoryCache = memoryCache;
        }

        // Retrieves all active cache entries with their keys and values.
        // GET /api/cache/all
        [HttpGet("All")]
        public IActionResult GetAllCacheKeys()
        {
            var cacheEntries = new List<object>();

            // Get all tracked cache keys.
            var cacheKeys = _cacheManager.GetAllKeys();

            foreach (var key in cacheKeys)
            {
                // Try to get the value from the memory cache.
                if (_memoryCache.TryGetValue(key, out object? value))
                {
                    // Add the key-value pair to the result list.
                    cacheEntries.Add(new { Key = key, Value = value });
                }
                else
                {
                    // If the key is not found in the memory cache, we can still return it as a tracked key.
                    cacheEntries.Add(new { Key = key, Value = "Not Found" });
                }
            }

            // Return the list of cache entries.
            return Ok(cacheEntries);
        }

        // TODO: Implement a method to clear all cache entries.
        // Retrieves a specific key's value
        // GET /api/cache/{key}
        [HttpGet("{key}")]
        public IActionResult GetCacheEntry(string key)
        {
            // Try to get the value from the cache.
            if (_memoryCache.TryGetValue(key, out object? value))
            {
                return Ok(new { Key = key, Value = value });
            }

            // If not found, return a NotFound result.
            return NotFound(new { Message = $"Cache key '{key}' not found"});
        }

        // Clears ALL cache entries
        // DELETE /api/cache/clearall
        [HttpDelete("ClearAll")]
        public IActionResult ClearAllCache()
        {
            try
            {
                // Clear all cache entries using the CacheManager.
                _cacheManager.ClearCache();

                return Ok(new { Message = "All cache entries cleared." });
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while clearing cache: {ex}", ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to clear cache." });
            }
        }

        // Clears a specific cache entry
        // DELETE /api/cache/{key}
        [HttpDelete("{key}")]
        public IActionResult ClearCacheByKey(string key)
        {
            try
            {
                // Check 
                if (_cacheManager.GetAllKeys().Contains(key))
                {
                    // Remove the specific cache entry using the CacheManager.
                    _cacheManager.Remove(key);

                    return Ok(new { Message = $"Cache entry '{key}' cleared." });
                }
                else
                {
                    return NotFound(new { Message = $"Cache key '{key}' not found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while clearing cache entry '{key}': {ex}", key, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Failed to clear cache entry '{key}'." });
            }
        }


    }
}
