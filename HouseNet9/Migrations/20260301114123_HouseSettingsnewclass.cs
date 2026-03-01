using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class HouseSettingsnewclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HouseSettingsId",
                table: "Houses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HouseSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepositPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DepositDueDays = table.Column<int>(type: "int", nullable: false),
                    FullPaymentDueDaysBeforeArrival = table.Column<int>(type: "int", nullable: false),
                    BankAccountIban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    BankAccountSwift = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    BankAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Houses_HouseSettingsId",
                table: "Houses",
                column: "HouseSettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Houses_HouseSettings_HouseSettingsId",
                table: "Houses",
                column: "HouseSettingsId",
                principalTable: "HouseSettings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Houses_HouseSettings_HouseSettingsId",
                table: "Houses");

            migrationBuilder.DropTable(
                name: "HouseSettings");

            migrationBuilder.DropIndex(
                name: "IX_Houses_HouseSettingsId",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "HouseSettingsId",
                table: "Houses");
        }
    }
}
