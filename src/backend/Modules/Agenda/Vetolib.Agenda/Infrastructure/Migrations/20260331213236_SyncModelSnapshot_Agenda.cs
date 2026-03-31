using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Agenda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelSnapshot_Agenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "follow_up_rules",
                schema: "agenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FollowUpDays = table.Column<int>(type: "integer", nullable: false),
                    FollowUpReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_follow_up_rules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_follow_up_rules_ClinicId_ConsultationType",
                schema: "agenda",
                table: "follow_up_rules",
                columns: new[] { "ClinicId", "ConsultationType" },
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "follow_up_rules",
                schema: "agenda");
        }
    }
}
