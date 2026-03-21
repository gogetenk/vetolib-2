using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace Vetolib.Messaging.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddWhatsAppAndChannel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add Channel column to conversations table
        migrationBuilder.AddColumn<string>(
            name: "Channel",
            schema: "messaging",
            table: "conversations",
            type: "text",
            nullable: false,
            defaultValue: "Portal");

        // Create whatsapp_business_accounts table
        migrationBuilder.CreateTable(
            name: "whatsapp_business_accounts",
            schema: "messaging",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                WabaId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PhoneNumberId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                EncryptedAccessToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_whatsapp_business_accounts", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_whatsapp_business_accounts_ClinicId",
            schema: "messaging",
            table: "whatsapp_business_accounts",
            column: "ClinicId",
            unique: true);

        // Create whatsapp_phone_mappings table
        migrationBuilder.CreateTable(
            name: "whatsapp_phone_mappings",
            schema: "messaging",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                OptInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_whatsapp_phone_mappings", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_whatsapp_phone_mappings_ClinicId_Phone",
            schema: "messaging",
            table: "whatsapp_phone_mappings",
            columns: new[] { "ClinicId", "Phone" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "whatsapp_phone_mappings",
            schema: "messaging");

        migrationBuilder.DropTable(
            name: "whatsapp_business_accounts",
            schema: "messaging");

        migrationBuilder.DropColumn(
            name: "Channel",
            schema: "messaging",
            table: "conversations");
    }
}
