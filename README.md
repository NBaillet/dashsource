# Train Journey Checker

A .NET Aspire application that scrapes train journey information from National Rail and stores it in a PostgreSQL database.

## Features

- Built with .NET 8 and Aspire for cloud-native orchestration
- PostgreSQL database for persistent storage
- Entity Framework Core with migrations
- Console application for scraping train journeys
- Duplicate detection to avoid saving the same journey multiple times
- Journey tracking with:
  - Departure and arrival times
  - Station information (from/to)
  - Delay information (on-time/delayed status and delay duration)
  - Scraping timestamp

## Prerequisites

- .NET 8 SDK
- Docker (for running PostgreSQL)
- Aspire workload (`dotnet workload install aspire`)

## Project Structure

- **TrainJourneyChecker.AppHost** - Aspire orchestration host
- **TrainJourneyChecker.ServiceDefaults** - Shared service configurations
- **TrainJourneyChecker.Scraper** - Console application for scraping journeys
- **TrainJourneyChecker.ApiService** - (Template-generated, not used)
- **TrainJourneyChecker.Web** - (Template-generated, not used)

## Running the Application

### Using Aspire (Recommended)

The AppHost is configured with default arguments. You can run the entire application stack including PostgreSQL:

```bash
dotnet run --project TrainJourneyChecker.AppHost
```

This will:
1. Start PostgreSQL in a container
2. Start PgAdmin for database management
3. Run the scraper with default arguments

### Running the Scraper Standalone

You can also run the scraper directly with custom arguments:

```bash
# Set the connection string
export ConnectionStrings__trainjourneydb="Host=localhost;Port=5432;Database=trainjourneydb;Username=postgres;Password=yourpassword"

# Run the scraper
cd TrainJourneyChecker.Scraper
dotnet run "London Euston" "Manchester Piccadilly" "2025-10-20 09:00"
```

### Command-Line Arguments

The scraper requires three arguments:

1. **From Station** - Starting station name (e.g., "London Euston")
2. **To Station** - Destination station name (e.g., "Manchester Piccadilly")
3. **Departure DateTime** - Departure date and time in format "yyyy-MM-dd HH:mm"

Example:
```bash
dotnet run "London Euston" "Manchester Piccadilly" "2025-10-20 09:00"
```

## Database

The application uses PostgreSQL with Entity Framework Core. Migrations are automatically applied on startup.

### Entity Structure

**Journey**
- Id (int, primary key)
- FromStation (string, required)
- ToStation (string, required)
- DepartureTime (DateTime, UTC)
- ArrivalTime (DateTime, UTC)
- IsOnTime (bool)
- DelayMinutes (int)
- ScrapedAt (DateTime, UTC)

### Duplicate Detection

The application checks for existing journeys with the same:
- From/To stations
- Departure and arrival times
- On-time status
- Delay minutes

If a matching journey exists, it won't be added again.

## Implementation Notes

### National Rail Scraping

The current implementation uses **mock data** for demonstration purposes. In a production environment, you would need to:

1. Use the National Rail Darwin API with proper credentials
2. Implement proper web scraping with authentication
3. Use a third-party API service

The mock implementation generates 10 journeys with varying departure times, durations, and delay statuses.

## Development

### Adding Migrations

```bash
cd TrainJourneyChecker.Scraper
dotnet ef migrations add MigrationName
```

### Building

```bash
dotnet build
```

### Testing

Run the scraper with different stations and times to verify functionality. The application will log:
- Journey additions
- Duplicate detections
- Database save operations

## Technologies Used

- .NET 8
- .NET Aspire 9.5
- Entity Framework Core 9.0
- PostgreSQL 8.0
- Npgsql
- HtmlAgilityPack (for future web scraping)
- Microsoft.Extensions.Hosting

## License

This project is for demonstration purposes.
