using HtmlAgilityPack;
using TrainJourneyChecker.Scraper.Models;
using System.Text.RegularExpressions;

namespace TrainJourneyChecker.Scraper.Services;

public class NationalRailScraper : IDisposable
{
    private readonly HttpClient _httpClient;
    private const int DefaultJourneyCount = 10;
    private bool _disposed = false;

    public NationalRailScraper(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<List<Journey>> ScrapeJourneysAsync(string fromStation, string toStation, DateTime departureDateTime)
    {
        var journeys = new List<Journey>();

        // Ensure departure datetime is in UTC for database storage
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
            Console.WriteLine("Attempting to scrape National Rail website...");
            
            // Attempt to fetch the journey planner page
            var searchUrl = BuildSearchUrl(fromStation, toStation, departureDateTime);
            Console.WriteLine($"Search URL: {searchUrl}");
            
            var response = await _httpClient.GetAsync(searchUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var htmlContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Successfully fetched National Rail page.");
                
                journeys = ParseJourneyResults(htmlContent, fromStation, toStation, departureDateTime);
                
                if (journeys.Count > 0)
                {
                    Console.WriteLine($"Parsed {journeys.Count} journeys from National Rail website.");
                }
                else
                {
                    Console.WriteLine("No journeys found in HTML response.");
                    Console.WriteLine("Falling back to mock data generation...");
                    journeys = GenerateMockJourneys(fromStation, toStation, departureDateTime);
                }
            }
            else
            {
                Console.WriteLine($"Failed to access National Rail website. Status: {response.StatusCode}");
                Console.WriteLine("Falling back to mock data generation...");
                journeys = GenerateMockJourneys(fromStation, toStation, departureDateTime);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error while accessing National Rail: {ex.Message}");
            Console.WriteLine("This could be due to:");
            Console.WriteLine("- No internet connection");
            Console.WriteLine("- The website is blocking automated requests");
            Console.WriteLine("- The website is temporarily unavailable");
            Console.WriteLine("Falling back to mock data generation...");
            journeys = GenerateMockJourneys(fromStation, toStation, departureDateTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during scraping: {ex.Message}");
            Console.WriteLine("Falling back to mock data generation...");
            journeys = GenerateMockJourneys(fromStation, toStation, departureDateTime);
        }

        return journeys;
    }

    private string BuildSearchUrl(string fromStation, string toStation, DateTime departureDateTime)
    {
        // Build URL for National Rail journey planner
        // Format: https://www.nationalrail.co.uk/journey-planner/
        // The actual API might be different - this is a starting point
        var from = Uri.EscapeDataString(fromStation);
        var to = Uri.EscapeDataString(toStation);
        var date = departureDateTime.ToString("ddMMyyyy");
        var hour = departureDateTime.ToString("HH");
        var minute = departureDateTime.ToString("mm");
        
        return $"https://www.nationalrail.co.uk/journey-planner/?type=single&from={from}&to={to}&leavingType=departing&leavingDate={date}&leavingHour={hour}&leavingMin={minute}";
    }

    private List<Journey> ParseJourneyResults(string htmlContent, string fromStation, string toStation, DateTime departureDateTime)
    {
        var journeys = new List<Journey>();

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Try to find journey results in the HTML
            // National Rail structure may vary, so we'll try multiple selectors
            var journeyNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'journey')] | //div[contains(@class, 'service')] | //tr[contains(@class, 'journey')]");

            if (journeyNodes != null)
            {
                Console.WriteLine($"Found {journeyNodes.Count} potential journey nodes.");
                
                foreach (var node in journeyNodes.Take(DefaultJourneyCount))
                {
                    var journey = ExtractJourneyFromNode(node, fromStation, toStation);
                    if (journey != null)
                    {
                        journeys.Add(journey);
                    }
                }
            }
            else
            {
                Console.WriteLine("No journey nodes found with expected HTML structure.");
            }

            // If no journeys found, try alternative parsing
            if (journeys.Count == 0)
            {
                journeys = TryAlternativeParsingMethods(doc, fromStation, toStation, departureDateTime);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing HTML: {ex.Message}");
        }

        return journeys;
    }

    private Journey? ExtractJourneyFromNode(HtmlNode node, string fromStation, string toStation)
    {
        try
        {
            // Try to extract departure and arrival times
            var timeNodes = node.SelectNodes(".//time | .//span[contains(@class, 'time')] | .//div[contains(@class, 'time')]");
            if (timeNodes != null && timeNodes.Count >= 2)
            {
                var departureTimeStr = timeNodes[0].InnerText?.Trim();
                var arrivalTimeStr = timeNodes[1].InnerText?.Trim();

                if (TryParseTime(departureTimeStr, out var departureTime) && 
                    TryParseTime(arrivalTimeStr, out var arrivalTime))
                {
                    // Check for delay information
                    var delayNode = node.SelectSingleNode(".//*[contains(@class, 'delay')] | .//*[contains(@class, 'late')] | .//*[contains(text(), 'late')] | .//*[contains(text(), 'delay')]");
                    var isDelayed = delayNode != null;
                    var delayMinutes = 0;

                    if (isDelayed && delayNode != null)
                    {
                        var delayText = delayNode.InnerText;
                        var delayMatch = Regex.Match(delayText, @"(\d+)");
                        if (delayMatch.Success)
                        {
                            int.TryParse(delayMatch.Groups[1].Value, out delayMinutes);
                        }
                    }

                    return new Journey
                    {
                        FromStation = fromStation,
                        ToStation = toStation,
                        DepartureTime = departureTime,
                        ArrivalTime = arrivalTime,
                        IsOnTime = !isDelayed,
                        DelayMinutes = delayMinutes,
                        ScrapedAt = DateTime.UtcNow
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting journey from node: {ex.Message}");
        }

        return null;
    }

    private List<Journey> TryAlternativeParsingMethods(HtmlDocument doc, string fromStation, string toStation, DateTime departureDateTime)
    {
        var journeys = new List<Journey>();

        try
        {
            // Look for time patterns in the HTML
            var timePattern = @"\b([0-1]?[0-9]|2[0-3]):[0-5][0-9]\b";
            var matches = Regex.Matches(doc.DocumentNode.InnerText, timePattern);

            if (matches.Count >= 2)
            {
                Console.WriteLine($"Found {matches.Count / 2} potential journey time pairs.");
                
                for (int i = 0; i < matches.Count - 1 && journeys.Count < DefaultJourneyCount; i += 2)
                {
                    if (TryParseTime(matches[i].Value, out var depTime) && 
                        TryParseTime(matches[i + 1].Value, out var arrTime))
                    {
                        journeys.Add(new Journey
                        {
                            FromStation = fromStation,
                            ToStation = toStation,
                            DepartureTime = depTime,
                            ArrivalTime = arrTime,
                            IsOnTime = true,
                            DelayMinutes = 0,
                            ScrapedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in alternative parsing: {ex.Message}");
        }

        return journeys;
    }

    private bool TryParseTime(string? timeStr, out DateTime time)
    {
        time = DateTime.UtcNow;
        
        if (string.IsNullOrWhiteSpace(timeStr))
            return false;

        try
        {
            // Clean the time string
            timeStr = Regex.Replace(timeStr, @"[^\d:]", "");
            
            if (DateTime.TryParse(timeStr, out var parsedTime))
            {
                // Convert to today's date with the parsed time, in UTC
                time = DateTime.SpecifyKind(
                    new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 
                                parsedTime.Hour, parsedTime.Minute, 0), 
                    DateTimeKind.Utc);
                return true;
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return false;
    }

    private List<Journey> GenerateMockJourneys(string fromStation, string toStation, DateTime departureDateTime)
    {
        var journeys = new List<Journey>();
        Console.WriteLine($"Generating {DefaultJourneyCount} mock journeys for demonstration...");

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
