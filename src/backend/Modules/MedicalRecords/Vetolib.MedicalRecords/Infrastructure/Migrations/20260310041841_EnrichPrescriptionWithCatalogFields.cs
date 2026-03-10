using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnrichPrescriptionWithCatalogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DrugCatalogEntryId",
                schema: "medical",
                table: "prescriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverrideJustification",
                schema: "medical",
                table: "prescriptions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverrideSeverity",
                schema: "medical",
                table: "prescriptions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StockDecrementConfirmed",
                schema: "medical",
                table: "prescriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DrugCatalogEntryId",
                schema: "medical",
                table: "prescriptions");

            migrationBuilder.DropColumn(
                name: "OverrideJustification",
                schema: "medical",
                table: "prescriptions");

            migrationBuilder.DropColumn(
                name: "OverrideSeverity",
                schema: "medical",
                table: "prescriptions");

            migrationBuilder.DropColumn(
                name: "StockDecrementConfirmed",
                schema: "medical",
                table: "prescriptions");
        }
    }
}
