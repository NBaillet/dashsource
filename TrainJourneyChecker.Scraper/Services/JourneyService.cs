using Microsoft.EntityFrameworkCore;
using TrainJourneyChecker.Scraper.Data;
using TrainJourneyChecker.Scraper.Models;

namespace TrainJourneyChecker.Scraper.Services;

public class JourneyService
{
    private readonly JourneyDbContext _context;

    public JourneyService(JourneyDbContext context)
    {
        _context = context;
    }

    public async Task SaveJourneysAsync(List<Journey> journeys)
    {
        foreach (var journey in journeys)
        {
            // Check if journey already exists with same values
            var exists = await _context.Journeys.AnyAsync(j =>
                j.FromStation == journey.FromStation &&
                j.ToStation == journey.ToStation &&
                j.DepartureTime == journey.DepartureTime &&
                j.ArrivalTime == journey.ArrivalTime &&
                j.IsOnTime == journey.IsOnTime &&
                j.DelayMinutes == journey.DelayMinutes);

            if (!exists)
            {
                _context.Journeys.Add(journey);
                Console.WriteLine($"Adding journey: {journey.FromStation} -> {journey.ToStation} at {journey.DepartureTime:yyyy-MM-dd HH:mm}");
            }
            else
            {
                Console.WriteLine($"Journey already exists: {journey.FromStation} -> {journey.ToStation} at {journey.DepartureTime:yyyy-MM-dd HH:mm}");
            }
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"Saved {journeys.Count} journeys to database.");
    }
}
