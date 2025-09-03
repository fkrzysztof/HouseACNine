using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class relationcontacthouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HouseId",
                table: "Contacts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_HouseId",
                table: "Contacts",
                column: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Houses_HouseId",
                table: "Contacts",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "HouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Houses_HouseId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_HouseId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "HouseId",
                table: "Contacts");
        }
    }
}
