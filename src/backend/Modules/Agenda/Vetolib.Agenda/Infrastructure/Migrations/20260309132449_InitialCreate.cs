using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "agenda");

            migrationBuilder.CreateTable(
                name: "appointments",
                schema: "agenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    VeterinarianId = table.Column<Guid>(type: "uuid", nullable: false),
                    VeterinarianName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_ClinicId_VeterinarianId_Date",
                schema: "agenda",
                table: "appointments",
                columns: new[] { "ClinicId", "VeterinarianId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointments",
                schema: "agenda");
        }
    }
}
