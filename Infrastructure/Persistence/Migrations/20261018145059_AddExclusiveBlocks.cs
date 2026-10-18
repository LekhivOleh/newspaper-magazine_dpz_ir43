using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CURSOVA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExclusiveBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExclusive",
                table: "ContentBlocks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Existing demo data: the gallery in the headphones review becomes exclusive media.
            migrationBuilder.Sql("""
                UPDATE ContentBlocks SET IsExclusive = 1
                WHERE Kind = 'gallery'
                  AND ArticleId IN (SELECT Id FROM Articles WHERE Slug = 'ohliad-bezdrotovykh-navushnykiv-serednoho-klasu');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExclusive",
                table: "ContentBlocks");
        }
    }
}
