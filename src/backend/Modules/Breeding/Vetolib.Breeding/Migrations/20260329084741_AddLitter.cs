using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Breeding.Migrations
{
    /// <inheritdoc />
    public partial class AddLitter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "breeding");

            migrationBuilder.CreateTable(
                name: "litters",
                schema: "breeding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    MotherPatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FatherPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalFatherName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BornCount = table.Column<int>(type: "integer", nullable: false),
                    AliveCount = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_litters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "litter_offspring",
                schema: "breeding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    LitterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    BirthOrder = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_litter_offspring", x => x.Id);
                    table.ForeignKey(
                        name: "FK_litter_offspring_litters_LitterId",
                        column: x => x.LitterId,
                        principalSchema: "breeding",
                        principalTable: "litters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_litter_offspring_litter_patient",
                schema: "breeding",
                table: "litter_offspring",
                columns: new[] { "LitterId", "PatientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_litters_clinic_mother",
                schema: "breeding",
                table: "litters",
                columns: new[] { "ClinicId", "MotherPatientId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "litter_offspring",
                schema: "breeding");

            migrationBuilder.DropTable(
                name: "litters",
                schema: "breeding");
        }
    }
}
