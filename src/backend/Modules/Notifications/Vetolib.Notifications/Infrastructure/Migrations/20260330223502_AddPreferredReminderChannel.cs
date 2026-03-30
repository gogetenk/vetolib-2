using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Notifications.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredReminderChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredReminderChannel",
                schema: "notifications",
                table: "reminder_configs",
                type: "text",
                nullable: false,
                defaultValue: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredReminderChannel",
                schema: "notifications",
                table: "reminder_configs");
        }
    }
}
