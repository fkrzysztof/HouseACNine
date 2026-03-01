using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class logoImgInSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HouseSettings_MyFiles_LogoFileId",
                table: "HouseSettings");

            migrationBuilder.DropIndex(
                name: "IX_HouseSettings_LogoFileId",
                table: "HouseSettings");

            migrationBuilder.DropColumn(
                name: "LogoFileId",
                table: "HouseSettings");

            migrationBuilder.AddColumn<string>(
                name: "LogoFileName",
                table: "HouseSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoFileName",
                table: "HouseSettings");

            migrationBuilder.AddColumn<int>(
                name: "LogoFileId",
                table: "HouseSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HouseSettings_LogoFileId",
                table: "HouseSettings",
                column: "LogoFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_HouseSettings_MyFiles_LogoFileId",
                table: "HouseSettings",
                column: "LogoFileId",
                principalTable: "MyFiles",
                principalColumn: "FileID");
        }
    }
}
