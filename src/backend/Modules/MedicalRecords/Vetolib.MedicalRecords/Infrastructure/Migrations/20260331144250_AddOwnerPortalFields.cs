using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.MedicalRecords.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerPortalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OwnerAccountId column and index already added by AddOwnerAccountIdToOwner migration.
            // Only add the IsVisibleToOwner column here.

            migrationBuilder.AddColumn<bool>(
                name: "IsVisibleToOwner",
                schema: "medical",
                table: "medical_records",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisibleToOwner",
                schema: "medical",
                table: "medical_records");
        }
    }
}
