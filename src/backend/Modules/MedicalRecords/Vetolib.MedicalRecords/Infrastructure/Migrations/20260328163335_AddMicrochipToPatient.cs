using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMicrochipToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MicrochipNumber",
                schema: "medical",
                table: "patients",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_patients_ClinicId_MicrochipNumber",
                schema: "medical",
                table: "patients",
                columns: new[] { "ClinicId", "MicrochipNumber" },
                unique: true,
                filter: "\"MicrochipNumber\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patients_ClinicId_MicrochipNumber",
                schema: "medical",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "MicrochipNumber",
                schema: "medical",
                table: "patients");
        }
    }
}
