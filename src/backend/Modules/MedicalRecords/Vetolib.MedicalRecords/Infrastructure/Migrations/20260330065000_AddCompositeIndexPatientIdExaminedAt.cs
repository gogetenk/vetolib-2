using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexPatientIdExaminedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_medical_records_PatientId",
                schema: "medical",
                table: "medical_records");

            migrationBuilder.CreateIndex(
                name: "IX_medical_records_PatientId_ExaminedAt",
                schema: "medical",
                table: "medical_records",
                columns: new[] { "PatientId", "ExaminedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_medical_records_PatientId_ExaminedAt",
                schema: "medical",
                table: "medical_records");

            migrationBuilder.CreateIndex(
                name: "IX_medical_records_PatientId",
                schema: "medical",
                table: "medical_records",
                column: "PatientId");
        }
    }
}
