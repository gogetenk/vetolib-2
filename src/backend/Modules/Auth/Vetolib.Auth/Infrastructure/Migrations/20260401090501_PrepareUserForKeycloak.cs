using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PrepareUserForKeycloak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "auth",
                table: "users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "AuthProvider",
                schema: "auth",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Legacy");

            migrationBuilder.CreateIndex(
                name: "IX_users_KeycloakUserId",
                schema: "auth",
                table: "users",
                column: "KeycloakUserId",
                unique: true,
                filter: "\"KeycloakUserId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_KeycloakUserId",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AuthProvider",
                schema: "auth",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "auth",
                table: "users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
