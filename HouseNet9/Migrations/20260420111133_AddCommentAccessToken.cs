using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentAccessToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommentAccessTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentAccessTokens_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentAccessTokens_CommentId",
                table: "CommentAccessTokens",
                column: "CommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentAccessTokens");
        }
    }
}
