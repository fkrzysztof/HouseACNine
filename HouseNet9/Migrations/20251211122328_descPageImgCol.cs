using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class descPageImgCol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MyFiles_DescriptionPageId",
                table: "MyFiles");

            migrationBuilder.CreateIndex(
                name: "IX_MyFiles_DescriptionPageId",
                table: "MyFiles",
                column: "DescriptionPageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MyFiles_DescriptionPageId",
                table: "MyFiles");

            migrationBuilder.CreateIndex(
                name: "IX_MyFiles_DescriptionPageId",
                table: "MyFiles",
                column: "DescriptionPageId",
                unique: true,
                filter: "[DescriptionPageId] IS NOT NULL");
        }
    }
}
