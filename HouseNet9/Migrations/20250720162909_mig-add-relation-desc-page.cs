using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class migaddrelationdescpage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DescriptionPageId",
                table: "MyFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HouseId",
                table: "DescriptionPages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MyFiles_DescriptionPageId",
                table: "MyFiles",
                column: "DescriptionPageId",
                unique: true,
                filter: "[DescriptionPageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionPages_HouseId",
                table: "DescriptionPages",
                column: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_DescriptionPages_Houses_HouseId",
                table: "DescriptionPages",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_MyFiles_DescriptionPages_DescriptionPageId",
                table: "MyFiles",
                column: "DescriptionPageId",
                principalTable: "DescriptionPages",
                principalColumn: "DescriptionPageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DescriptionPages_Houses_HouseId",
                table: "DescriptionPages");

            migrationBuilder.DropForeignKey(
                name: "FK_MyFiles_DescriptionPages_DescriptionPageId",
                table: "MyFiles");

            migrationBuilder.DropIndex(
                name: "IX_MyFiles_DescriptionPageId",
                table: "MyFiles");

            migrationBuilder.DropIndex(
                name: "IX_DescriptionPages_HouseId",
                table: "DescriptionPages");

            migrationBuilder.DropColumn(
                name: "DescriptionPageId",
                table: "MyFiles");

            migrationBuilder.DropColumn(
                name: "HouseId",
                table: "DescriptionPages");
        }
    }
}
