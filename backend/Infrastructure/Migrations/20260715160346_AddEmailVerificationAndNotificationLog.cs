using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationAndNotificationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailNotificationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeduplicationKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotificationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_CreatedAt",
                table: "EmailNotificationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_DeduplicationKey",
                table: "EmailNotificationLogs",
                column: "DeduplicationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_Recipient_EventType",
                table: "EmailNotificationLogs",
                columns: new[] { "RecipientEmail", "EventType" });

            // ── Seed: confirm all existing users so they are not locked out ──────────
            // Any user registered before this migration did not go through the email
            // verification flow, so we mark them as confirmed automatically.
            // New registrations after this migration MUST verify their email.
            migrationBuilder.Sql(
                "UPDATE AspNetUsers SET EmailConfirmed = 1 WHERE EmailConfirmed = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailNotificationLogs");
        }
    }
}
