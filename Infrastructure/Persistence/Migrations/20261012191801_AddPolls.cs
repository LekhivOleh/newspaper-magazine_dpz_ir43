using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CURSOVA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InteractiveElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ArticleId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParticipantCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Question = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    AllowMultiple = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveElements_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PollOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PollId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollOptions_InteractiveElements_PollId",
                        column: x => x.PollId,
                        principalTable: "InteractiveElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ElementId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipantKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    SelectedOptionIds = table.Column<string>(type: "TEXT", nullable: false),
                    ResultKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Score = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_InteractiveElements_ElementId",
                        column: x => x.ElementId,
                        principalTable: "InteractiveElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveElements_ArticleId_IsActive",
                table: "InteractiveElements",
                columns: new[] { "ArticleId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PollOptions_PollId",
                table: "PollOptions",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ElementId_ParticipantKey",
                table: "Submissions",
                columns: new[] { "ElementId", "ParticipantKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollOptions");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "InteractiveElements");
        }
    }
}
