using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_patients_ClinicId_Name",
                schema: "medical",
                table: "patients",
                columns: new[] { "ClinicId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_patients_ClinicId_Species",
                schema: "medical",
                table: "patients",
                columns: new[] { "ClinicId", "Species" });

            migrationBuilder.CreateIndex(
                name: "IX_owners_ClinicId_Phone",
                schema: "medical",
                table: "owners",
                columns: new[] { "ClinicId", "Phone" },
                filter: "\"Phone\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patients_ClinicId_Name",
                schema: "medical",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_patients_ClinicId_Species",
                schema: "medical",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_owners_ClinicId_Phone",
                schema: "medical",
                table: "owners");
        }
    }
}
