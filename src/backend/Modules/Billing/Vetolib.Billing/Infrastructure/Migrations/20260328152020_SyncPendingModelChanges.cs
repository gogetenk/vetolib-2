using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "billing",
                table: "invoice_items");

            migrationBuilder.CreateTable(
                name: "ereporting_periods",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlatformSubmissionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TransactionCount = table.Column<int>(type: "integer", nullable: false),
                    TotalExclTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalInclTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ereporting_periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ereporting_tax_breakdowns",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EReportingPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    TaxCategory = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ereporting_tax_breakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ereporting_tax_breakdowns_ereporting_periods_EReportingPeri~",
                        column: x => x.EReportingPeriodId,
                        principalSchema: "billing",
                        principalTable: "ereporting_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ereporting_periods_ClinicId_PeriodStart_PeriodEnd",
                schema: "billing",
                table: "ereporting_periods",
                columns: new[] { "ClinicId", "PeriodStart", "PeriodEnd" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ereporting_tax_breakdowns_EReportingPeriodId",
                schema: "billing",
                table: "ereporting_tax_breakdowns",
                column: "EReportingPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ereporting_tax_breakdowns",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "ereporting_periods",
                schema: "billing");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                schema: "billing",
                table: "invoice_items",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }
    }
}
