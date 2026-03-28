using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Messaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_messages_ConversationId",
                schema: "messaging",
                table: "messages");

            migrationBuilder.AlterColumn<double>(
                name: "ClassifiedConfidence",
                schema: "messaging",
                table: "messages",
                type: "double precision",
                precision: 5,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,4)",
                oldPrecision: 5,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                schema: "messaging",
                table: "conversations",
                type: "text",
                nullable: false,
                defaultValue: "Portal");

            migrationBuilder.AddColumn<bool>(
                name: "IsSpam",
                schema: "messaging",
                table: "conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "pending_uploads",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_uploads", x => x.Id);
                });

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
                name: "IX_messages_ConversationId_SentAt",
                schema: "messaging",
                table: "messages",
                columns: new[] { "ConversationId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_message_attachments_MessageId",
                schema: "messaging",
                table: "message_attachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_ClinicId_CreatedAt",
                schema: "messaging",
                table: "conversations",
                columns: new[] { "ClinicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_pending_uploads_ExpiresAt",
                schema: "messaging",
                table: "pending_uploads",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_business_accounts_ClinicId",
                schema: "messaging",
                table: "whatsapp_business_accounts",
                column: "ClinicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_phone_mappings_ClinicId_Phone",
                schema: "messaging",
                table: "whatsapp_phone_mappings",
                columns: new[] { "ClinicId", "Phone" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_message_attachments_messages_MessageId",
                schema: "messaging",
                table: "message_attachments",
                column: "MessageId",
                principalSchema: "messaging",
                principalTable: "messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_message_attachments_messages_MessageId",
                schema: "messaging",
                table: "message_attachments");

            migrationBuilder.DropTable(
                name: "pending_uploads",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "whatsapp_business_accounts",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "whatsapp_phone_mappings",
                schema: "messaging");

            migrationBuilder.DropIndex(
                name: "IX_messages_ConversationId_SentAt",
                schema: "messaging",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "IX_message_attachments_MessageId",
                schema: "messaging",
                table: "message_attachments");

            migrationBuilder.DropIndex(
                name: "IX_conversations_ClinicId_CreatedAt",
                schema: "messaging",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "Channel",
                schema: "messaging",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "IsSpam",
                schema: "messaging",
                table: "conversations");

            migrationBuilder.AlterColumn<decimal>(
                name: "ClassifiedConfidence",
                schema: "messaging",
                table: "messages",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 5,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_messages_ConversationId",
                schema: "messaging",
                table: "messages",
                column: "ConversationId");
        }
    }
}
