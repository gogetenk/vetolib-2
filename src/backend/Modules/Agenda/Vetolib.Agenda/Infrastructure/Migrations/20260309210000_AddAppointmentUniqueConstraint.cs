using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <summary>
    /// Adds a partial unique index on (VeterinarianId, Date, StartTime) WHERE Status != 'Cancelled'.
    /// This prevents TOCTOU double booking at the DB level even under concurrent requests.
    /// The index is partial so that cancelled appointments do not block re-use of a time slot.
    /// </summary>
    public partial class AddAppointmentUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_appointments_vet_date_starttime_unique""
                ON agenda.appointments (""VeterinarianId"", ""Date"", ""StartTime"")
                WHERE ""Status"" != 'Cancelled';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS agenda.""IX_appointments_vet_date_starttime_unique"";
            ");
        }
    }
}
