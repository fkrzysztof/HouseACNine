using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class removeismainaddorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMain",
                table: "MyFiles");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "MyFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "MyFiles");

            migrationBuilder.AddColumn<bool>(
                name: "IsMain",
                table: "MyFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
