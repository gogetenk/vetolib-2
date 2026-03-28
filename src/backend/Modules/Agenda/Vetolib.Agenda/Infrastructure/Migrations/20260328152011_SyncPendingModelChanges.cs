using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_appointments_ClinicId_Date_StartTime",
                schema: "agenda",
                table: "appointments",
                columns: new[] { "ClinicId", "Date", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_appointments_ClinicId_Date_StartTime",
                schema: "agenda",
                table: "appointments");
        }
    }
}
