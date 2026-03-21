using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace Vetolib.Messaging.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPendingUploads : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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

        migrationBuilder.CreateIndex(
            name: "IX_pending_uploads_ExpiresAt",
            schema: "messaging",
            table: "pending_uploads",
            column: "ExpiresAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "pending_uploads",
            schema: "messaging");
    }
}
