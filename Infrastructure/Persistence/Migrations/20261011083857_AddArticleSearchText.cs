using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CURSOVA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleSearchText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "Articles",
                type: "TEXT",
                maxLength: 801,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "Articles");
        }
    }
}
