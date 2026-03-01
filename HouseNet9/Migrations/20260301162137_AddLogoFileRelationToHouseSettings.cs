using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoFileRelationToHouseSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "HouseSettings");

            migrationBuilder.AddColumn<int>(
                name: "HouseSettingsId",
                table: "MyFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogoFileId",
                table: "HouseSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MyFiles_HouseSettingsId",
                table: "MyFiles",
                column: "HouseSettingsId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MyFiles_HouseSettings_HouseSettingsId",
                table: "MyFiles",
                column: "HouseSettingsId",
                principalTable: "HouseSettings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HouseSettings_MyFiles_LogoFileId",
                table: "HouseSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_MyFiles_HouseSettings_HouseSettingsId",
                table: "MyFiles");

            migrationBuilder.DropIndex(
                name: "IX_MyFiles_HouseSettingsId",
                table: "MyFiles");

            migrationBuilder.DropIndex(
                name: "IX_HouseSettings_LogoFileId",
                table: "HouseSettings");

            migrationBuilder.DropColumn(
                name: "HouseSettingsId",
                table: "MyFiles");

            migrationBuilder.DropColumn(
                name: "LogoFileId",
                table: "HouseSettings");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "HouseSettings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
