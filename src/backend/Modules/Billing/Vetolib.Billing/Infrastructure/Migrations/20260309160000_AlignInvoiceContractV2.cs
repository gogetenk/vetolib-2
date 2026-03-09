using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignInvoiceContractV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new patient/owner columns to invoices
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                schema: "billing",
                table: "invoices",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                schema: "billing",
                table: "invoices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                schema: "billing",
                table: "invoices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPhone",
                schema: "billing",
                table: "invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId",
                schema: "billing",
                table: "invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "billing",
                table: "invoices",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                schema: "billing",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: true);

            // Add Quantity column to invoice_items
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                schema: "billing",
                table: "invoice_items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Drop the old AnimalId column (data is not migrated for MVP)
            migrationBuilder.DropColumn(
                name: "AnimalId",
                schema: "billing",
                table: "invoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AnimalId",
                schema: "billing",
                table: "invoices",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.DropColumn(name: "PatientId", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "PatientName", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "OwnerName", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "OwnerPhone", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "AppointmentId", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "Notes", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "PaidAt", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "Quantity", schema: "billing", table: "invoice_items");
        }
    }
}
