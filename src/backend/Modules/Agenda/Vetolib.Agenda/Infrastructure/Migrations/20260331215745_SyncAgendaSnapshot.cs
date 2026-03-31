using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncAgendaSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty — follow_up_rules table was already created by AddFollowUpRules migration (20260331201305).
            // This migration only exists to re-sync the model snapshot which was broken by a concurrent
            // worktree merge where AddWaitingRoomAtToAppointment's Designer.cs didn't include FollowUpRule.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Empty — nothing to revert.
        }
    }
}
