using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationExpiry",
                schema: "auth",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                schema: "auth",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                schema: "auth",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_users_EmailVerificationToken",
                schema: "auth",
                table: "users",
                column: "EmailVerificationToken",
                filter: "\"EmailVerificationToken\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_EmailVerificationToken",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationExpiry",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                schema: "auth",
                table: "users");
        }
    }
}
