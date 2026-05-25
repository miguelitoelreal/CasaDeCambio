using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitoringPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SmtpHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SmtpPort = table.Column<int>(type: "integer", nullable: false),
                    SmtpUsername = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SmtpPassword = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SenderEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SenderName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    UseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantSettings_TenantId",
                table: "TenantSettings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantSettings");
        }
    }
}
