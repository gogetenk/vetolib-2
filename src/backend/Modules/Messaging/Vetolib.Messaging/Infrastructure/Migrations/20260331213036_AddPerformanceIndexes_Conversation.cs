using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Messaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes_Conversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_conversations_ClinicId_Status_Category",
                schema: "messaging",
                table: "conversations",
                columns: new[] { "ClinicId", "Status", "Category" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_conversations_ClinicId_Status_Category",
                schema: "messaging",
                table: "conversations");
        }
    }
}
