using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRecordTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "medical_record_templates",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiagnosisTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TreatmentTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    NotesTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Species = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsSystemTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_record_templates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_medical_record_templates_ClinicId_Category",
                schema: "medical",
                table: "medical_record_templates",
                columns: new[] { "ClinicId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_medical_record_templates_ClinicId_IsSystemTemplate",
                schema: "medical",
                table: "medical_record_templates",
                columns: new[] { "ClinicId", "IsSystemTemplate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "medical_record_templates",
                schema: "medical");
        }
    }
}
