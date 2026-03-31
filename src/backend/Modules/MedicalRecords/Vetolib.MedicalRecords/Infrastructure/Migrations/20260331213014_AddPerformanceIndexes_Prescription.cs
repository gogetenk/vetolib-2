using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes_Prescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_ClinicId_CreatedAt",
                schema: "medical",
                table: "prescriptions",
                columns: new[] { "ClinicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_ClinicId_MedicalRecordId",
                schema: "medical",
                table: "prescriptions",
                columns: new[] { "ClinicId", "MedicalRecordId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_prescriptions_ClinicId_CreatedAt",
                schema: "medical",
                table: "prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_prescriptions_ClinicId_MedicalRecordId",
                schema: "medical",
                table: "prescriptions");
        }
    }
}
