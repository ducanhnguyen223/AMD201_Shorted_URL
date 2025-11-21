using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShortenerService.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdAndCustomAlias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomAlias",
                table: "ShortenedUrls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ShortenedUrls",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomAlias",
                table: "ShortenedUrls");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ShortenedUrls");
        }
    }
}
