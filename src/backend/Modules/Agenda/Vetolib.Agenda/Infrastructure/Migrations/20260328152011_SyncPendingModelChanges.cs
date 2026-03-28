using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Columns from orphaned migrations (missing Designer files) ──

            // From AddReminderFields
            migrationBuilder.AddColumn<string>(
                name: "OwnerEmail",
                schema: "agenda",
                table: "appointments",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                schema: "agenda",
                table: "appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // From AddBookingSourceRescheduleFields
            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "agenda",
                table: "appointments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Staff");

            migrationBuilder.AddColumn<int>(
                name: "RescheduleCount",
                schema: "agenda",
                table: "appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalAppointmentId",
                schema: "agenda",
                table: "appointments",
                type: "uuid",
                nullable: true);

            // From AddConsultationTypes
            migrationBuilder.CreateTable(
                name: "consultation_types",
                schema: "agenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RequiresVetSelection = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultation_types", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consultation_types_ClinicId_Name",
                schema: "agenda",
                table: "consultation_types",
                columns: new[] { "ClinicId", "Name" },
                unique: true,
                filter: "\"IsActive\" = true");

            // From AddAppointmentUniqueConstraint
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_appointments_vet_date_starttime_unique""
                ON agenda.appointments (""VeterinarianId"", ""Date"", ""StartTime"")
                WHERE ""Status"" != 'Cancelled';
            ");

            // ── Original SyncPendingModelChanges content ──

            migrationBuilder.CreateIndex(
                name: "IX_appointments_ClinicId_Date_StartTime",
                schema: "agenda",
                table: "appointments",
                columns: new[] { "ClinicId", "Date", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_appointments_ClinicId_Date_StartTime",
                schema: "agenda",
                table: "appointments");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS agenda.""IX_appointments_vet_date_starttime_unique"";
            ");

            migrationBuilder.DropTable(
                name: "consultation_types",
                schema: "agenda");

            migrationBuilder.DropColumn(name: "OwnerEmail", schema: "agenda", table: "appointments");
            migrationBuilder.DropColumn(name: "ReminderSent", schema: "agenda", table: "appointments");
            migrationBuilder.DropColumn(name: "Source", schema: "agenda", table: "appointments");
            migrationBuilder.DropColumn(name: "RescheduleCount", schema: "agenda", table: "appointments");
            migrationBuilder.DropColumn(name: "OriginalAppointmentId", schema: "agenda", table: "appointments");
        }
    }
}
