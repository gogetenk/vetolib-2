using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Billing.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddEInvoicingGatewayFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "EInvoicingStatus",
            schema: "billing",
            table: "invoices",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PlatformInvoiceId",
            schema: "billing",
            table: "invoices",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EInvoicingStatus",
            schema: "billing",
            table: "invoices");

        migrationBuilder.DropColumn(
            name: "PlatformInvoiceId",
            schema: "billing",
            table: "invoices");
    }
}
