using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecurityFixes_UniqueTokenAndGlobalEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_ClinicId_Email",
                schema: "auth",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_Token",
                schema: "auth",
                table: "refresh_tokens");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "auth",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                schema: "auth",
                table: "refresh_tokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                schema: "auth",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_Token",
                schema: "auth",
                table: "refresh_tokens");

            migrationBuilder.CreateIndex(
                name: "IX_users_ClinicId_Email",
                schema: "auth",
                table: "users",
                columns: new[] { "ClinicId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                schema: "auth",
                table: "refresh_tokens",
                column: "Token");
        }
    }
}
