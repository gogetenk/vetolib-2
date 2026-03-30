using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWaitlistEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "waitlist_entries",
                schema: "agenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OwnerPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PreferredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PreferredTimeSlot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VetPreference = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waitlist_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_waitlist_entries_ClinicId_Status_PreferredDate",
                schema: "agenda",
                table: "waitlist_entries",
                columns: new[] { "ClinicId", "Status", "PreferredDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "waitlist_entries",
                schema: "agenda");
        }
    }
}
