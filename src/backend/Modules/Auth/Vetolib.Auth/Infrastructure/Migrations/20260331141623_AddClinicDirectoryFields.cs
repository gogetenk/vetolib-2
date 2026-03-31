using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicDirectoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "auth",
                table: "clinics",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                schema: "auth",
                table: "clinics",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "auth",
                table: "clinics",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "SupportedSpecies",
                schema: "auth",
                table: "clinics",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateIndex(
                name: "IX_clinics_City",
                schema: "auth",
                table: "clinics",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_clinics_Slug",
                schema: "auth",
                table: "clinics",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clinics_City",
                schema: "auth",
                table: "clinics");

            migrationBuilder.DropIndex(
                name: "IX_clinics_Slug",
                schema: "auth",
                table: "clinics");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "auth",
                table: "clinics");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                schema: "auth",
                table: "clinics");

            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "auth",
                table: "clinics");

            migrationBuilder.DropColumn(
                name: "SupportedSpecies",
                schema: "auth",
                table: "clinics");
        }
    }
}
