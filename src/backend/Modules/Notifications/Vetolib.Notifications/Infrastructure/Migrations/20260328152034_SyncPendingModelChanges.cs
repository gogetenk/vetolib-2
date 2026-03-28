using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Notifications.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reminder_configs",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Appointment24hEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    VaccinationDueEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Appointment24hLeadTimeHours = table.Column<int>(type: "integer", nullable: false),
                    VaccinationDueLeadTimeDays = table.Column<int>(type: "integer", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminder_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reminder_logs",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReminderType = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "text", nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminder_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reminder_configs_ClinicId",
                schema: "notifications",
                table: "reminder_configs",
                column: "ClinicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reminder_logs_AppointmentId_ReminderType",
                schema: "notifications",
                table: "reminder_logs",
                columns: new[] { "AppointmentId", "ReminderType" });

            migrationBuilder.CreateIndex(
                name: "IX_reminder_logs_ClinicId",
                schema: "notifications",
                table: "reminder_logs",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_logs_PatientId_ReminderType",
                schema: "notifications",
                table: "reminder_logs",
                columns: new[] { "PatientId", "ReminderType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reminder_configs",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "reminder_logs",
                schema: "notifications");
        }
    }
}
