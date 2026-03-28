using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clinic_groups",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinic_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clinic_group_members",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinic_group_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clinic_group_members_clinic_groups_ClinicGroupId",
                        column: x => x.ClinicGroupId,
                        principalSchema: "auth",
                        principalTable: "clinic_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinic_group_members_ClinicGroupId_ClinicId",
                schema: "auth",
                table: "clinic_group_members",
                columns: new[] { "ClinicGroupId", "ClinicId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinic_group_members",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "clinic_groups",
                schema: "auth");
        }
    }
}
