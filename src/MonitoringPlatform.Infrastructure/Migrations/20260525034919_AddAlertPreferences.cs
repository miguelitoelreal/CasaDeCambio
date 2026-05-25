using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitoringPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecipientEmails",
                table: "AlertRules",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedCloudProviderIds",
                table: "AlertRules",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ThrottleMinutes",
                table: "AlertRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserAlertPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MonitorDownAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CloudIncidentCriticalAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CloudIncidentMajorAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    WeeklySummaryEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    WeeklySummaryDay = table.Column<int>(type: "integer", nullable: false),
                    WeeklySummaryIncludeMonitors = table.Column<bool>(type: "boolean", nullable: false),
                    WeeklySummaryIncludeCloud = table.Column<bool>(type: "boolean", nullable: false),
                    SelectedCloudProviderIds = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AdditionalEmails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAlertPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAlertPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAlertPreferences_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAlertPreferences_TenantId",
                table: "UserAlertPreferences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAlertPreferences_UserId_TenantId",
                table: "UserAlertPreferences",
                columns: new[] { "UserId", "TenantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAlertPreferences");

            migrationBuilder.DropColumn(
                name: "RecipientEmails",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "SelectedCloudProviderIds",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "ThrottleMinutes",
                table: "AlertRules");
        }
    }
}
