using HtmlAgilityPack;
using TrainJourneyChecker.Scraper.Models;

namespace TrainJourneyChecker.Scraper.Services;

public class NationalRailScraper
{
    private readonly HttpClient _httpClient;
    private const int DefaultJourneyCount = 10;

    public NationalRailScraper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<List<Journey>> ScrapeJourneysAsync(string fromStation, string toStation, DateTime departureDateTime)
    {
        var journeys = new List<Journey>();

        // Ensure departure datetime is in UTC
        if (departureDateTime.Kind == DateTimeKind.Unspecified)
        {
            departureDateTime = DateTime.SpecifyKind(departureDateTime, DateTimeKind.Utc);
        }
        else if (departureDateTime.Kind == DateTimeKind.Local)
        {
            departureDateTime = departureDateTime.ToUniversalTime();
        }

        try
        {
            // Note: This is a mock implementation for demonstration purposes
            // The actual National Rail website requires authentication or an API key
            // In a production environment, you would either:
            // 1. Use the National Rail Darwin API with proper credentials
            // 2. Use a web scraping approach with proper handling of their website structure
            // 3. Use a third-party API service
            
            // For now, generate mock journey data for demonstration
            Console.WriteLine($"Generating mock journey data from {fromStation} to {toStation}...");
            
            journeys = GenerateMockJourneys(fromStation, toStation, departureDateTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: Using mock data due to: {ex.Message}");
            journeys = GenerateMockJourneys(fromStation, toStation, departureDateTime);
        }

        return Task.FromResult(journeys);
    }

    private List<Journey> GenerateMockJourneys(string fromStation, string toStation, DateTime departureDateTime)
    {
        var journeys = new List<Journey>();

        for (int i = 0; i < DefaultJourneyCount; i++)
        {
            var departure = departureDateTime.AddMinutes(i * 30);
            var duration = 60 + (i * 5); // Journey duration varies
            var arrival = departure.AddMinutes(duration);
            var isDelayed = i % 3 == 0; // Every third journey is delayed
            var delayMinutes = isDelayed ? 5 + (i * 2) : 0;

            journeys.Add(new Journey
            {
                FromStation = fromStation,
                ToStation = toStation,
                DepartureTime = departure,
                ArrivalTime = arrival.AddMinutes(delayMinutes),
                IsOnTime = !isDelayed,
                DelayMinutes = delayMinutes,
                ScrapedAt = DateTime.UtcNow
            });
        }

        return journeys;
    }
}
