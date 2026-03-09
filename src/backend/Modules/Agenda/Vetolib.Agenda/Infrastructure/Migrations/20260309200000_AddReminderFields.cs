using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerEmail",
                schema: "agenda",
                table: "appointments",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                schema: "agenda",
                table: "appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerEmail",
                schema: "agenda",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "ReminderSent",
                schema: "agenda",
                table: "appointments");
        }
    }
}
