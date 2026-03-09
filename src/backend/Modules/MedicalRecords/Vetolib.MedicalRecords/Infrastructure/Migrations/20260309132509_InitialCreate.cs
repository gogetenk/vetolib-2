using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "medical");

            migrationBuilder.CreateTable(
                name: "owners",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_owners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Species = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Breed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "medical_records",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Diagnosis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Treatment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    VetName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExaminedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medical_records_patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "medical",
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_owners",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_owners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_owners_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "medical",
                        principalTable: "owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_patient_owners_patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "medical",
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prescriptions",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicalRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Medication = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Dosage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    VetLicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prescriptions_medical_records_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalSchema: "medical",
                        principalTable: "medical_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_medical_records_PatientId",
                schema: "medical",
                table: "medical_records",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_owners_ClinicId_Email",
                schema: "medical",
                table: "owners",
                columns: new[] { "ClinicId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_owners_OwnerId",
                schema: "medical",
                table: "patient_owners",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_owners_PatientId_OwnerId",
                schema: "medical",
                table: "patient_owners",
                columns: new[] { "PatientId", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_MedicalRecordId",
                schema: "medical",
                table: "prescriptions",
                column: "MedicalRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patient_owners",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "prescriptions",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "owners",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "medical_records",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "patients",
                schema: "medical");
        }
    }
}
