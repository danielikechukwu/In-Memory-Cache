using InMemoryCachingExecution.Data;
using InMemoryCachingExecution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.CompilerServices;

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

                // Set the cache with the fetched data.
                // Manual eviction means: do not set any expiration time
                // _cache.Set(cacheKey, countries, _cacheExpiration); // with expiration
                _cache.Set("countries", countries); // without expiration
            }

            // Returns the cached or fresh data.
            return countries ?? new List<Country>();

        }

        // Method to remove countries from the cache manually
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

            RemoveCountriesFromCache();

        }

        // Update a Country and then clear the cache
        public async Task UpdateCountry(Country updateCountry)
        {
            _context.Countries.Update(updateCountry);

            await _context.SaveChangesAsync();

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
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10));

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
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(10));

                // Set the cache with the fetched data and expiration time.
                _cache.Set(cacheKey, cities, cacheEntryOptions);
            }
            
            // Returns the cached or fresh data.
            return cities ?? new List<City>();
        }

    }
}
