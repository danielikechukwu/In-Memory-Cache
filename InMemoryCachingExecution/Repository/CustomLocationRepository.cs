using InMemoryCachingExecution.Data;
using InMemoryCachingExecution.Models;
using InMemoryCachingExecution.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCachingExecution.Repository
{
    public class CustomLocationRepository
    {
        // InMemoryCachingDbContext instance for interacting with the database.
        private readonly InMemoryCachingDbContext _context;

        // Custom cache manager to handle caching.
        private readonly CacheManager _cache;

        // Cache expiration duration.
        private readonly int _CacheAbsoluteDurationMinutes;
        private readonly int _CacheSlidingDurationMinutes;

        // Configuration for reading settings from appsettings.json.
        private readonly IConfiguration _configuration;

        // Cache expiration time set to 30 minutes.
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        // Constructor that accepts InMemoryCachingDbContext and IMemoryCache instances.
        public CustomLocationRepository(InMemoryCachingDbContext context, CacheManager cache, IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _configuration = configuration;

            // Read the cache expiration durations with default fallbacks.
            _CacheAbsoluteDurationMinutes = _configuration.GetValue<int?>("CacheSettings:AbsoluteExpirationMinutes") ?? 30;

            _CacheSlidingDurationMinutes = _configuration.GetValue<int?>("CacheSettings:SlidingExpirationMinutes") ?? 30;
        }

        // Retrieves all countries from the database, with caching.
        // 1. Manual Eviction for Countries (Caching implementation)
        public async Task<List<Country>> GetAllCountriesAsync()
        {

            // Define a unique key for caching countries data
            var cacheKey = "countries";

            // Try to get the cached list of countries.
            if (!_cache.TryGetValue(cacheKey, out List<Country>? countries))
            {
                // If not found in cache, fetch from the database.
                countries = await _context.Countries
                    .AsNoTracking() // Improves performance for read-only queries
                    .ToListAsync();

                // Set cache entry options with high priority.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.High); // Countries are considered critical data.

                // Set the cache with the fetched data.
                // Manual eviction means: do not set any expiration time
                // _cache.Set(cacheKey, countries, _cacheExpiration); // with expiration
                // _cache.Set("countries", countries); // without expiration

                // Cache the countries without explicit expiration.
                _cache.Set(cacheKey, countries, cacheEntryOptions);
            }

            // Returns the cached or fresh data.
            return countries ?? new List<Country>();

        }

        // Removes the countries cache entry.
        // This is useful after any data modification.
        public void RemoveCountriesFromCache()
        {
            var cacheKey = "countries";

            // Remove the cached countries data
            _cache.Remove(cacheKey);
        }

        // Add a Country and then clear the cache
        public async Task AddCountry(Country country)
        {
            _context.Countries.Add(country);

            await _context.SaveChangesAsync();

            // Clear cache so that subsequent reads get fresh data.
            RemoveCountriesFromCache();

        }

        // Update a Country and then clear the cache
        public async Task UpdateCountry(Country updateCountry)
        {
            _context.Countries.Update(updateCountry);

            await _context.SaveChangesAsync();

            // Clear cache after update.
            RemoveCountriesFromCache();

        }

        // Retrieves all states from the database, with caching.
        // 2. Sliding Expiration for States
        public async Task<List<State>> GetAllStatesAsync(int countryId)
        {
            // Unique cache key for states based on country ID.
            var cacheKey = $"States_{countryId}";

            // Try to get the cached list of states.
            if (!_cache.TryGetValue(cacheKey, out List<State>? states))
            {
                // If not found in cache, fetch from the database.
                states = await _context.States
                    .Where(s => s.CountryId == countryId) // Filter states by country ID
                    .AsNoTracking() // Improves performance for read-only queries
                    .ToListAsync();

                // Configure sliding expiration
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(_CacheSlidingDurationMinutes))
                    .SetPriority(CacheItemPriority.Normal);

                // Set the cache with the fetched data and expiration time.
                _cache.Set(cacheKey, states, cacheEntryOptions);
            }

            // Returns the cached or fresh data.
            return states ?? new List<State>();
        }

        // Retrieves the list of cities for a specific state, with caching.
        // 3. Absolute Expiration for Cities
        public async Task<List<City>> GetCitiesByStateAsync(int stateId)
        {
            // Unique cache key for cities based on state ID.
            var cacheKey = $"Cities_{stateId}";

            // Try to get the cached list of cities.
            if (!_cache.TryGetValue(cacheKey, out List<City>? cities))
            {

                // If not found in cache, fetch from the database.
                cities = await _context.Cities
                    .Where(c => c.StateId == stateId) // Filter cities by state ID
                    .AsNoTracking() // Improves performance for read-only queries
                    .ToListAsync();

                // Configure absolute expiration
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_CacheAbsoluteDurationMinutes))
                    .SetPriority(CacheItemPriority.Low);

                // Set the cache with the fetched data and expiration time.
                _cache.Set(cacheKey, cities, cacheEntryOptions);
            }

            // Returns the cached or fresh data.
            return cities ?? new List<City>();
        }

    }
}
