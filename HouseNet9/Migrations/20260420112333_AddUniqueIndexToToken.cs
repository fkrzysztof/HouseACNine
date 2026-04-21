using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "CommentAccessTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CommentAccessTokens_Token",
                table: "CommentAccessTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommentAccessTokens_Token",
                table: "CommentAccessTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "CommentAccessTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
