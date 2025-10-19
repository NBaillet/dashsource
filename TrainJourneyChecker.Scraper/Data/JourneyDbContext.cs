using Microsoft.EntityFrameworkCore;
using TrainJourneyChecker.Scraper.Models;

namespace TrainJourneyChecker.Scraper.Data;

public class JourneyDbContext : DbContext
{
    public JourneyDbContext(DbContextOptions<JourneyDbContext> options) : base(options)
    {
    }

    public DbSet<Journey> Journeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Journey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromStation).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ToStation).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DepartureTime).IsRequired();
            entity.Property(e => e.ArrivalTime).IsRequired();
            entity.Property(e => e.IsOnTime).IsRequired();
            entity.Property(e => e.DelayMinutes).IsRequired();
            entity.Property(e => e.ScrapedAt).IsRequired();

            entity.HasIndex(e => new { e.FromStation, e.ToStation, e.DepartureTime });
        });
    }
}
