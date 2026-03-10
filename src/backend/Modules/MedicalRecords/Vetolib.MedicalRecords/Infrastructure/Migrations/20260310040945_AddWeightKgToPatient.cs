using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightKgToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                schema: "medical",
                table: "patients",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "drug_catalog_entries",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    InnName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drug_catalog_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "drug_dosage_guidelines",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCatalogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Species = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinDosePerKg = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    MaxDosePerKg = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Route = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drug_dosage_guidelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_drug_dosage_guidelines_drug_catalog_entries_DrugCatalogEntr~",
                        column: x => x.DrugCatalogEntryId,
                        principalSchema: "medical",
                        principalTable: "drug_catalog_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "drug_interactions",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCatalogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OtherDrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    OtherDrugName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drug_interactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_drug_interactions_drug_catalog_entries_DrugCatalogEntryId",
                        column: x => x.DrugCatalogEntryId,
                        principalSchema: "medical",
                        principalTable: "drug_catalog_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "drug_species_contraindications",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCatalogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Species = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drug_species_contraindications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_drug_species_contraindications_drug_catalog_entries_DrugCat~",
                        column: x => x.DrugCatalogEntryId,
                        principalSchema: "medical",
                        principalTable: "drug_catalog_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_drug_catalog_entries_ClinicId_InnName",
                schema: "medical",
                table: "drug_catalog_entries",
                columns: new[] { "ClinicId", "InnName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drug_catalog_entries_InnName",
                schema: "medical",
                table: "drug_catalog_entries",
                column: "InnName");

            migrationBuilder.CreateIndex(
                name: "IX_drug_dosage_guidelines_DrugCatalogEntryId",
                schema: "medical",
                table: "drug_dosage_guidelines",
                column: "DrugCatalogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_drug_interactions_DrugCatalogEntryId",
                schema: "medical",
                table: "drug_interactions",
                column: "DrugCatalogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_drug_species_contraindications_DrugCatalogEntryId",
                schema: "medical",
                table: "drug_species_contraindications",
                column: "DrugCatalogEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "drug_dosage_guidelines",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "drug_interactions",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "drug_species_contraindications",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "drug_catalog_entries",
                schema: "medical");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                schema: "medical",
                table: "patients");
        }
    }
}
