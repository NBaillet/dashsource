var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var database = postgres.AddDatabase("trainjourneydb");

builder.AddProject<Projects.TrainJourneyChecker_Scraper>("scraper")
    .WithReference(database);

builder.Build().Run();
