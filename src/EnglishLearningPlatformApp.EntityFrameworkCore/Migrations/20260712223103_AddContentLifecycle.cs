using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningPlatformApp.Migrations
{
    /// <inheritdoc />
    public partial class AddContentLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppContentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ChangeSequence = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContentItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppContentVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Lifecycle = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppContentVersions_AppContentItems_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "AppContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppContentItems_TenantId_Status",
                table: "AppContentItems",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppContentVersions_ContentItemId",
                table: "AppContentVersions",
                column: "ContentItemId",
                unique: true,
                filter: "[Lifecycle] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AppContentVersions_ContentItemId_VersionNumber",
                table: "AppContentVersions",
                columns: new[] { "ContentItemId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppContentVersions_TenantId_Lifecycle",
                table: "AppContentVersions",
                columns: new[] { "TenantId", "Lifecycle" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppContentVersions");

            migrationBuilder.DropTable(
                name: "AppContentItems");
        }
    }
}
