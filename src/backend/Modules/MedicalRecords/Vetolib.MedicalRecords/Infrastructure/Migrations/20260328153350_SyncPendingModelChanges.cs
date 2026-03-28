using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockDecrementConfirmed",
                schema: "medical",
                table: "prescriptions");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                schema: "medical",
                table: "patients",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AlternativeDrugId",
                schema: "medical",
                table: "drug_species_contraindications",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternativeDrugId",
                schema: "medical",
                table: "drug_species_contraindications");

            migrationBuilder.AddColumn<bool>(
                name: "StockDecrementConfirmed",
                schema: "medical",
                table: "prescriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                schema: "medical",
                table: "patients",
                type: "numeric(8,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }
    }
}
