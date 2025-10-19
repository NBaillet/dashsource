var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");

var database = postgres.AddDatabase("trainjourneydb");

builder.AddProject<Projects.DashSource_Scraper>("scraper")
    .WithReference(database)
    .WithArgs("London Euston", "Manchester Piccadilly", "2025-10-20 09:00");

builder.Build().Run();
