using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                schema: "agenda",
                table: "appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_appointments_ClinicId_OwnerId",
                schema: "agenda",
                table: "appointments",
                columns: new[] { "ClinicId", "OwnerId" },
                filter: "\"OwnerId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_appointments_ClinicId_OwnerId",
                schema: "agenda",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "agenda",
                table: "appointments");
        }
    }
}
