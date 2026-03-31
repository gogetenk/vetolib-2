using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedRecordLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shared_record_links",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccessCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_record_links", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shared_record_links_ClinicId_PatientId",
                schema: "medical",
                table: "shared_record_links",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_shared_record_links_OwnerAccountId_Active",
                schema: "medical",
                table: "shared_record_links",
                columns: new[] { "OwnerAccountId", "RevokedAt", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_shared_record_links_Token",
                schema: "medical",
                table: "shared_record_links",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_record_links",
                schema: "medical");
        }
    }
}
