using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTaxFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                schema: "billing",
                table: "invoices",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "AE");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                schema: "billing",
                table: "invoice_items",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0.05m);

            migrationBuilder.AddColumn<string>(
                name: "TaxCategory",
                schema: "billing",
                table: "invoice_items",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Standard");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                schema: "billing",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                schema: "billing",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "TaxCategory",
                schema: "billing",
                table: "invoice_items");
        }
    }
}
