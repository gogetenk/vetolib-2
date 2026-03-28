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

            migrationBuilder.AddColumn<bool>(
                name: "IsSpam",
                schema: "messaging",
                table: "conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
