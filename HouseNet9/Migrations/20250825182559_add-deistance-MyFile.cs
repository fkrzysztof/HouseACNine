using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class adddeistanceMyFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Distances_MyFiles_ImageFileID",
                table: "Distances");

            migrationBuilder.DropIndex(
                name: "IX_Distances_ImageFileID",
                table: "Distances");

            migrationBuilder.DropColumn(
                name: "ImageFileID",
                table: "Distances");

            migrationBuilder.AddColumn<int>(
                name: "DistanceID",
                table: "MyFiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MyFiles_DistanceID",
                table: "MyFiles",
                column: "DistanceID",
                unique: true,
                filter: "[DistanceID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_MyFiles_Distances_DistanceID",
                table: "MyFiles",
                column: "DistanceID",
                principalTable: "Distances",
                principalColumn: "DistanceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MyFiles_Distances_DistanceID",
                table: "MyFiles");

            migrationBuilder.DropIndex(
                name: "IX_MyFiles_DistanceID",
                table: "MyFiles");

            migrationBuilder.DropColumn(
                name: "DistanceID",
                table: "MyFiles");

            migrationBuilder.AddColumn<int>(
                name: "ImageFileID",
                table: "Distances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Distances_ImageFileID",
                table: "Distances",
                column: "ImageFileID");

            migrationBuilder.AddForeignKey(
                name: "FK_Distances_MyFiles_ImageFileID",
                table: "Distances",
                column: "ImageFileID",
                principalTable: "MyFiles",
                principalColumn: "FileID");
        }
    }
}
