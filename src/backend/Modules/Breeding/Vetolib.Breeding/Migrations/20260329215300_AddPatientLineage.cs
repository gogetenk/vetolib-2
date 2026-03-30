using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Breeding.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientLineage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patient_lineages",
                schema: "breeding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    MotherPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    FatherPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistryNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegistryType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_lineages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_patient_lineages_clinic_father",
                schema: "breeding",
                table: "patient_lineages",
                columns: new[] { "ClinicId", "FatherPatientId" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_lineages_clinic_mother",
                schema: "breeding",
                table: "patient_lineages",
                columns: new[] { "ClinicId", "MotherPatientId" });

            migrationBuilder.CreateIndex(
                name: "ix_patient_lineages_clinic_patient",
                schema: "breeding",
                table: "patient_lineages",
                columns: new[] { "ClinicId", "PatientId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patient_lineages",
                schema: "breeding");
        }
    }
}
