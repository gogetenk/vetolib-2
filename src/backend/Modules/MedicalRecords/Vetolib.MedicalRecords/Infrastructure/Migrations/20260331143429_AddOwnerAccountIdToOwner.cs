using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerAccountIdToOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerAccountId",
                schema: "medical",
                table: "owners",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_owners_OwnerAccountId",
                schema: "medical",
                table: "owners",
                column: "OwnerAccountId",
                filter: "\"OwnerAccountId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_owners_OwnerAccountId",
                schema: "medical",
                table: "owners");

            migrationBuilder.DropColumn(
                name: "OwnerAccountId",
                schema: "medical",
                table: "owners");
        }
    }
}
