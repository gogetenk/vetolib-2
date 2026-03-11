using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSourceRescheduleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "agenda",
                table: "appointments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Staff");

            migrationBuilder.AddColumn<int>(
                name: "RescheduleCount",
                schema: "agenda",
                table: "appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalAppointmentId",
                schema: "agenda",
                table: "appointments",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                schema: "agenda",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "RescheduleCount",
                schema: "agenda",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "OriginalAppointmentId",
                schema: "agenda",
                table: "appointments");
        }
    }
}
