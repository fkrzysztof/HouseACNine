using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseIdToRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DescriptionPages_Houses_HouseId",
                table: "DescriptionPages");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailedInformation_Houses_HouseId",
                table: "DetailedInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_Distances_Houses_HouseId",
                table: "Distances");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralInformation_Houses_HouseId",
                table: "GeneralInformation");

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "GeneralInformation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "Distances",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "DetailedInformation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "DescriptionPages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DescriptionPages_Houses_HouseId",
                table: "DescriptionPages",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailedInformation_Houses_HouseId",
                table: "DetailedInformation",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Distances_Houses_HouseId",
                table: "Distances",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralInformation_Houses_HouseId",
                table: "GeneralInformation",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DescriptionPages_Houses_HouseId",
                table: "DescriptionPages");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailedInformation_Houses_HouseId",
                table: "DetailedInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_Distances_Houses_HouseId",
                table: "Distances");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralInformation_Houses_HouseId",
                table: "GeneralInformation");

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "GeneralInformation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "Distances",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "DetailedInformation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "DescriptionPages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_DescriptionPages_Houses_HouseId",
                table: "DescriptionPages",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetailedInformation_Houses_HouseId",
                table: "DetailedInformation",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Distances_Houses_HouseId",
                table: "Distances",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralInformation_Houses_HouseId",
                table: "GeneralInformation",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId");
        }
    }
}
