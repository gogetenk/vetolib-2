using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace Vetolib.Messaging.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddMessageClassification : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ClassifiedUrgency",
            schema: "messaging",
            table: "messages",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ClassifiedCategory",
            schema: "messaging",
            table: "messages",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "ClassifiedConfidence",
            schema: "messaging",
            table: "messages",
            type: "numeric(5,4)",
            precision: 5,
            scale: 4,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsFlaggedForReview",
            schema: "messaging",
            table: "messages",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<Guid>(
            name: "OverriddenByUserId",
            schema: "messaging",
            table: "messages",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OriginalAiUrgency",
            schema: "messaging",
            table: "messages",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OriginalAiCategory",
            schema: "messaging",
            table: "messages",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "ClassificationFeedbackCorrect",
            schema: "messaging",
            table: "messages",
            type: "boolean",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_messages_IsFlaggedForReview",
            schema: "messaging",
            table: "messages",
            column: "IsFlaggedForReview");

        migrationBuilder.CreateIndex(
            name: "IX_messages_ClassifiedCategory",
            schema: "messaging",
            table: "messages",
            column: "ClassifiedCategory");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_messages_ClassifiedCategory",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropIndex(
            name: "IX_messages_IsFlaggedForReview",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropColumn(
            name: "ClassificationFeedbackCorrect",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropColumn(
            name: "OriginalAiCategory",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropColumn(
            name: "OriginalAiUrgency",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropColumn(
            name: "OverriddenByUserId",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropColumn(
            name: "IsFlaggedForReview",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropColumn(
            name: "ClassifiedConfidence",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropColumn(
            name: "ClassifiedCategory",
            schema: "messaging",
            table: "messages");

        migrationBuilder.DropColumn(
            name: "ClassifiedUrgency",
            schema: "messaging",
            table: "messages");
    }
}
