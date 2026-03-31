using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientTransferSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TransferredAt",
                schema: "medical",
                table: "patients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferredToClinicId",
                schema: "medical",
                table: "patients",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "transfer_logs",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransferredBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transfer_logs_SourceClinicId_PatientId",
                schema: "medical",
                table: "transfer_logs",
                columns: new[] { "SourceClinicId", "PatientId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transfer_logs",
                schema: "medical");

            migrationBuilder.DropColumn(
                name: "TransferredAt",
                schema: "medical",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "TransferredToClinicId",
                schema: "medical",
                table: "patients");
        }
    }
}
