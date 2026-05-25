using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitoringPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSummaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeeklySummaryIncludeMonitors",
                table: "UserAlertPreferences",
                newName: "SummaryIncludeMonitors");

            migrationBuilder.RenameColumn(
                name: "WeeklySummaryIncludeCloud",
                table: "UserAlertPreferences",
                newName: "SummaryIncludeCloud");

            migrationBuilder.RenameColumn(
                name: "WeeklySummaryEnabled",
                table: "UserAlertPreferences",
                newName: "SummaryEnabled");

            migrationBuilder.RenameColumn(
                name: "WeeklySummaryDay",
                table: "UserAlertPreferences",
                newName: "SummaryFrequency");

            migrationBuilder.AddColumn<int>(
                name: "SummaryDay",
                table: "UserAlertPreferences",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummaryDay",
                table: "UserAlertPreferences");

            migrationBuilder.RenameColumn(
                name: "SummaryIncludeMonitors",
                table: "UserAlertPreferences",
                newName: "WeeklySummaryIncludeMonitors");

            migrationBuilder.RenameColumn(
                name: "SummaryIncludeCloud",
                table: "UserAlertPreferences",
                newName: "WeeklySummaryIncludeCloud");

            migrationBuilder.RenameColumn(
                name: "SummaryFrequency",
                table: "UserAlertPreferences",
                newName: "WeeklySummaryDay");

            migrationBuilder.RenameColumn(
                name: "SummaryEnabled",
                table: "UserAlertPreferences",
                newName: "WeeklySummaryEnabled");
        }
    }
}
