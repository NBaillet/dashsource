using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DashSource.Scraper.Data;
using DashSource.Scraper.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<JourneyDbContext>("trainjourneydb");

builder.Services.AddHttpClient<NationalRailScraper>();
builder.Services.AddScoped<JourneyService>();

var host = builder.Build();

// Ensure database is created and migrations are applied
// Wait for database to be ready with retry logic
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JourneyDbContext>();
    
    const int maxRetries = 10;
    const int delayMs = 2000;
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            Console.WriteLine($"Attempting to connect to database (attempt {i + 1}/{maxRetries})...");
            await context.Database.MigrateAsync();
            Console.WriteLine("Database migrations completed successfully.");
            break;
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            Console.WriteLine($"Database not ready yet: {ex.Message}");
            Console.WriteLine($"Waiting {delayMs}ms before retry...");
            await Task.Delay(delayMs);
        }
    }
}

// Parse command-line arguments
if (args.Length < 3)
{
    Console.WriteLine("Usage: DashSource.Scraper <from-station> <to-station> <departure-datetime>");
    Console.WriteLine("Example: DashSource.Scraper \"London Euston\" \"Manchester Piccadilly\" \"2025-10-20 09:00\"");
    return 1;
}

var fromStation = args[0];
var toStation = args[1];
var departureDateTimeStr = args[2];

if (!DateTime.TryParse(departureDateTimeStr, out var departureDateTime))
{
    Console.WriteLine($"Invalid datetime format: {departureDateTimeStr}");
    Console.WriteLine("Please use format: yyyy-MM-dd HH:mm");
    return 1;
}

Console.WriteLine($"Searching for journeys from {fromStation} to {toStation} departing after {departureDateTime:yyyy-MM-dd HH:mm}");

// Run the scraper
using (var scope = host.Services.CreateScope())
{
    var scraper = scope.ServiceProvider.GetRequiredService<NationalRailScraper>();
    var journeyService = scope.ServiceProvider.GetRequiredService<JourneyService>();

    var journeys = await scraper.ScrapeJourneysAsync(fromStation, toStation, departureDateTime);
    Console.WriteLine($"Found {journeys.Count} journeys");

    // Store journeys in database
    await journeyService.SaveJourneysAsync(journeys);
    Console.WriteLine("Journey scraping completed successfully!");
}

Console.WriteLine();
Console.WriteLine("Note: This application attempts to scrape the National Rail website.");
Console.WriteLine("If the website blocks automated requests or the structure has changed,");
Console.WriteLine("it will fall back to generating mock data for demonstration purposes.");

return 0;

