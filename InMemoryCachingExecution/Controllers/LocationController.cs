using InMemoryCachingExecution.Models;
using InMemoryCachingExecution.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InMemoryCachingExecution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {

        private readonly ILogger<LocationController> _logger;
        private readonly LocationRepository _locationRepository;

        // The repository is injected via constructor
        public LocationController(ILogger<LocationController> logger, LocationRepository locationRepository)
        {
            _logger = logger;
            _locationRepository = locationRepository;
        }

        // Retrieves all countries.
        // GET: api/location/countries
        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            List<Country> countries = await _locationRepository.GetAllCountriesAsync();

            if (countries == null || !countries.Any())
            {
                return NotFound("No countries found.");
            }

            return Ok(countries);

        }

        //Add a New Country
        [HttpPost("countries")]
        public async Task<IActionResult> AddCountry([FromBody] Country country)
        {

            try
            {
                await _locationRepository.AddCountry(country);

                return Ok(); // Indicates success with No Data to Return
            }
            catch(Exception ex)
            {
                _logger.LogError("An error occurred upon creation: {ex}", ex.Message);

                var customResponse = new
                {
                    Code = 500,
                    Message = "Internal Server Error",
                    // Do not expose the actual error to the client
                    ErrorMessage = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, customResponse);
            }
        }

        // Retrieves states by country ID.
        // GET: api/location/states/{countryId}
        [HttpGet("states/{countryId}")]
        public async Task<ActionResult<IEnumerable<State>>> GetStates(int countryId)
        {
            List<State> states = await _locationRepository.GetAllStatesAsync(countryId);

            if (states == null || !states.Any())
            {
                return NotFound($"No states found for country ID {countryId}");
            }

            return Ok(states);
        }

        // Retrieves cities by state ID.
        // GET: api/location/cities/{stateId}
        [HttpGet("cities/{stateId}")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities(int stateId)
        {
            List<City> cities = await _locationRepository.GetCitiesByStateAsync(stateId);

            if (cities == null || !cities.Any())
            {
                return NotFound($"No cities found for state ID {stateId}");
            }

            return Ok(cities);
        }
    }
}
