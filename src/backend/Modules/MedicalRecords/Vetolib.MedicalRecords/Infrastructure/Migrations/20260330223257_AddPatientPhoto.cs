using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoBase64",
                schema: "medical",
                table: "patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoContentType",
                schema: "medical",
                table: "patients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoBase64",
                schema: "medical",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "PhotoContentType",
                schema: "medical",
                table: "patients");
        }
    }
}
