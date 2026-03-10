using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetolib.Preferences.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPreferencesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "preferences");

            migrationBuilder.CreateTable(
                name: "clinic_preference_defaults",
                schema: "preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinic_preference_defaults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "consent_audit_entries",
                schema: "preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreviousValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consent_audit_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_preferences",
                schema: "preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clinic_preference_defaults_clinic_key",
                schema: "preferences",
                table: "clinic_preference_defaults",
                columns: new[] { "ClinicId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_consent_audit_entries_clinic_user_created",
                schema: "preferences",
                table: "consent_audit_entries",
                columns: new[] { "ClinicId", "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_user_preferences_clinic_user_key",
                schema: "preferences",
                table: "user_preferences",
                columns: new[] { "ClinicId", "UserId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinic_preference_defaults",
                schema: "preferences");

            migrationBuilder.DropTable(
                name: "consent_audit_entries",
                schema: "preferences");

            migrationBuilder.DropTable(
                name: "user_preferences",
                schema: "preferences");
        }
    }
}
