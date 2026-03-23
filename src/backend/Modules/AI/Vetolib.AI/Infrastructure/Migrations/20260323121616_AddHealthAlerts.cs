using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.AI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ai");

            migrationBuilder.CreateTable(
                name: "HealthAlerts",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RecommendedAction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RuleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DismissedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DismissedReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DismissedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConvertedToAppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "triage_results",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symptoms = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Species = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Breed = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AgeMonths = table.Column<int>(type: "integer", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: true),
                    SuggestedSeverity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    RecommendedSpecialty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Reasoning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    WasAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    OverriddenSeverity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ModelUsed = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PromptTokens = table.Column<int>(type: "integer", nullable: false),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: false),
                    LatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_triage_results", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthAlerts_ClinicId_PatientId_RuleId_Status",
                schema: "ai",
                table: "HealthAlerts",
                columns: new[] { "ClinicId", "PatientId", "RuleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_HealthAlerts_ClinicId_Status_Severity",
                schema: "ai",
                table: "HealthAlerts",
                columns: new[] { "ClinicId", "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_triage_results_ClinicId",
                schema: "ai",
                table: "triage_results",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_triage_results_CreatedAt",
                schema: "ai",
                table: "triage_results",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthAlerts",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "triage_results",
                schema: "ai");
        }
    }
}
