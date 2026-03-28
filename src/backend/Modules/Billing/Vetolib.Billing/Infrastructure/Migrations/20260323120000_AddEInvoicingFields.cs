using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEInvoicingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerSiren",
                schema: "billing",
                table: "invoices",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerVatNumber",
                schema: "billing",
                table: "invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerSiren",
                schema: "billing",
                table: "invoices",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerVatNumber",
                schema: "billing",
                table: "invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                schema: "billing",
                table: "invoices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuyerAddress",
                schema: "billing",
                table: "invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationType",
                schema: "billing",
                table: "invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceTypeCode",
                schema: "billing",
                table: "invoices",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "380");

            migrationBuilder.AddColumn<string>(
                name: "PaymentTerms",
                schema: "billing",
                table: "invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderReference",
                schema: "billing",
                table: "invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SellerSiren", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "SellerVatNumber", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "BuyerSiren", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "BuyerVatNumber", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "BuyerName", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "BuyerAddress", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "OperationType", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "InvoiceTypeCode", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "PaymentTerms", schema: "billing", table: "invoices");
            migrationBuilder.DropColumn(name: "PurchaseOrderReference", schema: "billing", table: "invoices");
        }
    }
}
