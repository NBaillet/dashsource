namespace TrainJourneyChecker.Scraper.Models;

public class Journey
{
    public int Id { get; set; }
    public string FromStation { get; set; } = string.Empty;
    public string ToStation { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public bool IsOnTime { get; set; }
    public int DelayMinutes { get; set; }
    public DateTime ScrapedAt { get; set; }
}
