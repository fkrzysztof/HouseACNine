using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class addLongShortTextrentalrules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Houses",
                newName: "ShortText");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Houses",
                newName: "RentalRules");

            migrationBuilder.AddColumn<string>(
                name: "LongText",
                table: "Houses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongText",
                table: "Houses");

            migrationBuilder.RenameColumn(
                name: "ShortText",
                table: "Houses",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "RentalRules",
                table: "Houses",
                newName: "Description");
        }
    }
}
