using HtmlAgilityPack;
using TrainJourneyChecker.Scraper.Models;

namespace TrainJourneyChecker.Scraper.Services;

public class NationalRailScraper
{
    private readonly HttpClient _httpClient;

    public NationalRailScraper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Journey>> ScrapeJourneysAsync(string fromStation, string toStation, DateTime departureDateTime)
    {
        var journeys = new List<Journey>();

        try
        {
            // National Rail Enquiries URL format
            // Note: This is a simplified approach. The actual National Rail website might require more complex scraping
            var url = $"https://www.nationalrail.co.uk/journey-planner/?type=single&from={Uri.EscapeDataString(fromStation)}&to={Uri.EscapeDataString(toStation)}&leavingType=departing&leavingDate={departureDateTime:ddMMyyyy}&leavingHour={departureDateTime:HH}&leavingMin={departureDateTime:mm}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // This is a mock implementation since actual National Rail scraping requires dealing with their API or complex HTML structure
            // For demonstration purposes, we'll create some sample data
            // In a real implementation, you would parse the HTML or use their API
            
            // Generate 10 mock journeys for demonstration
            for (int i = 0; i < 10; i++)
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping journeys: {ex.Message}");
            throw;
        }

        return journeys;
    }
}
