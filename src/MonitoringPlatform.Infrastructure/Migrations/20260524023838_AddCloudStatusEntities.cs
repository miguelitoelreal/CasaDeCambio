using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitoringPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudStatusEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CloudProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StatusPageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSyncError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CloudIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CloudProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Region = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AffectedServicesJson = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OfficialUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CloudIncidents_CloudProviders_CloudProviderId",
                        column: x => x.CloudProviderId,
                        principalTable: "CloudProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CloudIncidents_CloudProviderId_ExternalId",
                table: "CloudIncidents",
                columns: new[] { "CloudProviderId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CloudIncidents_CloudProviderId_IsActive",
                table: "CloudIncidents",
                columns: new[] { "CloudProviderId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CloudProviders_Slug",
                table: "CloudProviders",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudIncidents");

            migrationBuilder.DropTable(
                name: "CloudProviders");
        }
    }
}
