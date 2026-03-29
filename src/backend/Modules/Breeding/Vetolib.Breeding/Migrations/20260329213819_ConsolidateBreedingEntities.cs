using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Breeding.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateBreedingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeatCycles",
                schema: "breeding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeatCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pregnancies",
                schema: "breeding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FatherPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    MatingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MatingMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpectedDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ActualDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OffspringCount = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pregnancies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pregnancy_checks",
                schema: "breeding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PregnancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Result = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pregnancy_checks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pregnancy_checks_pregnancies_PregnancyId",
                        column: x => x.PregnancyId,
                        principalSchema: "breeding",
                        principalTable: "pregnancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeatCycles_PatientId_StartDate",
                schema: "breeding",
                table: "HeatCycles",
                columns: new[] { "PatientId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "ix_pregnancies_clinic_patient_status",
                schema: "breeding",
                table: "pregnancies",
                columns: new[] { "ClinicId", "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_pregnancy_checks_pregnancy_id",
                schema: "breeding",
                table: "pregnancy_checks",
                column: "PregnancyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeatCycles",
                schema: "breeding");

            migrationBuilder.DropTable(
                name: "pregnancy_checks",
                schema: "breeding");

            migrationBuilder.DropTable(
                name: "pregnancies",
                schema: "breeding");
        }
    }
}
