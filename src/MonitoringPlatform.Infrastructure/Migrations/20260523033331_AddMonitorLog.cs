using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitoringPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonitorLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitorLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitorLogs_Monitors_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "Monitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitorLogs_MonitorId",
                table: "MonitorLogs",
                column: "MonitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitorLogs");
        }
    }
}
