namespace InMemoryCachingExecution.Models
{
    public class Country
    {
        public int CountryId { get; set; } // Unique identifier for the country

        public string Name { get; set; } = string.Empty;

        // Navigation property to states belonging to this country
        public List<State> States { get; set; } = new List<State>();
    }
}
