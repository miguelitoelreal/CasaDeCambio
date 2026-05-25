using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitoringPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMicrosoftIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MicrosoftIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MicrosoftTenantId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ClientSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicrosoftIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MicrosoftIntegrations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MicrosoftIntegrations_TenantId",
                table: "MicrosoftIntegrations",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MicrosoftIntegrations");
        }
    }
}
