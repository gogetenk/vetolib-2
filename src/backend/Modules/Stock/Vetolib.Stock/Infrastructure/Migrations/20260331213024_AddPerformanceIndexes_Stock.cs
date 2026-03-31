using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Stock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes_Stock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_stock_items_ClinicId_ExpiryDate",
                schema: "stock",
                table: "stock_items",
                columns: new[] { "ClinicId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_items_ClinicId_Quantity",
                schema: "stock",
                table: "stock_items",
                columns: new[] { "ClinicId", "Quantity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stock_items_ClinicId_ExpiryDate",
                schema: "stock",
                table: "stock_items");

            migrationBuilder.DropIndex(
                name: "IX_stock_items_ClinicId_Quantity",
                schema: "stock",
                table: "stock_items");
        }
    }
}
