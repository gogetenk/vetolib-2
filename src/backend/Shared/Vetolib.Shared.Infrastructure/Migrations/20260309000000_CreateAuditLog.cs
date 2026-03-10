using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Vetolib.Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "shared");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId   = table.Column<string>(type: "character varying(50)",  maxLength: 50,  nullable: false),
                    Action     = table.Column<string>(type: "character varying(20)",  maxLength: 20,  nullable: false),
                    ChangedBy  = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ClinicId   = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp  = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldValues  = table.Column<string>(type: "text", nullable: true),
                    NewValues  = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_ClinicId_Timestamp",
                schema: "shared",
                table: "audit_log",
                columns: new[] { "ClinicId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_EntityType_EntityId",
                schema: "shared",
                table: "audit_log",
                columns: new[] { "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "audit_log", schema: "shared");
        }
    }
}
