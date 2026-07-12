using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningPlatformApp.Migrations
{
    /// <inheritdoc />
    public partial class AddContentQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppContentQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContentSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AnswerDefinitionJson = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContentQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppContentQuestions_AppContentSections_ContentSectionId",
                        column: x => x.ContentSectionId,
                        principalTable: "AppContentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppContentQuestionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MatchText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContentQuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppContentQuestionOptions_AppContentQuestions_ContentQuestionId",
                        column: x => x.ContentQuestionId,
                        principalTable: "AppContentQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppContentQuestionOptions_ContentQuestionId_Position",
                table: "AppContentQuestionOptions",
                columns: new[] { "ContentQuestionId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_AppContentQuestionOptions_TenantId_ContentQuestionId",
                table: "AppContentQuestionOptions",
                columns: new[] { "TenantId", "ContentQuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppContentQuestions_ContentSectionId_Position",
                table: "AppContentQuestions",
                columns: new[] { "ContentSectionId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_AppContentQuestions_TenantId_ContentSectionId",
                table: "AppContentQuestions",
                columns: new[] { "TenantId", "ContentSectionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppContentQuestionOptions");

            migrationBuilder.DropTable(
                name: "AppContentQuestions");
        }
    }
}
