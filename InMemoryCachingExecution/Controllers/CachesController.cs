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


    }
}
