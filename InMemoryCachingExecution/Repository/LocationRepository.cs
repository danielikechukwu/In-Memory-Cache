using InMemoryCachingExecution.Data;
using InMemoryCachingExecution.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCachingExecution.Repository
{
    public class LocationRepository
    {
        // InMemoryCachingDbContext instance for interacting with the database.
        private readonly InMemoryCachingDbContext _context;

        // IMemoryCache instance for implementing in-memory caching.
        private readonly IMemoryCache _cache;

        // Cache expiration time set to 30 minutes.
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        // Constructor that accepts InMemoryCachingDbContext and IMemoryCache instances.
        public LocationRepository(InMemoryCachingDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Retrieves all countries from the database, with caching.
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

                // Set the cache with the fetched data and expiration time.
                _cache.Set("countries", countries, _cacheExpiration);
            }

            // Returns the cached or fresh data.
            return countries ?? new List<Country>();

        }

        // Retrieves all states from the database, with caching.
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

                // Set the cache with the fetched data and expiration time.
                _cache.Set(cacheKey, states, _cacheExpiration);
            }

            // Returns the cached or fresh data.
            return states ?? new List<State>();
        }

        // Retrieves the list of cities for a specific state, with caching.
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

                // Set the cache with the fetched data and expiration time.
                _cache.Set(cacheKey, cities, _cacheExpiration);
            }
            
            // Returns the cached or fresh data.
            return cities ?? new List<City>();
        }

    }
}
