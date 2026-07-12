using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningPlatformApp.Migrations
{
    /// <inheritdoc />
    public partial class AddContentSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppContentSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Heading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppContentSections_AppContentVersions_ContentVersionId",
                        column: x => x.ContentVersionId,
                        principalTable: "AppContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppContentSections_ContentVersionId_Position",
                table: "AppContentSections",
                columns: new[] { "ContentVersionId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_AppContentSections_TenantId_ContentVersionId",
                table: "AppContentSections",
                columns: new[] { "TenantId", "ContentVersionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppContentSections");
        }
    }
}
