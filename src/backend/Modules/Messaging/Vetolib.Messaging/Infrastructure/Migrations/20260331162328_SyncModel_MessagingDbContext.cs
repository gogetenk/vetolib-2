using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Messaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel_MessagingDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_conversations_ClinicId_OwnerId",
                schema: "messaging",
                table: "conversations",
                columns: new[] { "ClinicId", "OwnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_conversations_ClinicId_OwnerId",
                schema: "messaging",
                table: "conversations");
        }
    }
}
