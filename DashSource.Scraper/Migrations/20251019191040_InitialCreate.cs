using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DashSource.Scraper.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Journeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromStation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToStation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsOnTime = table.Column<bool>(type: "boolean", nullable: false),
                    DelayMinutes = table.Column<int>(type: "integer", nullable: false),
                    ScrapedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_FromStation_ToStation_DepartureTime",
                table: "Journeys",
                columns: new[] { "FromStation", "ToStation", "DepartureTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Journeys");
        }
    }
}
