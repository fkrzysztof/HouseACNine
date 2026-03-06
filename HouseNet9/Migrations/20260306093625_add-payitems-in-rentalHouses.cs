using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class addpayitemsinrentalHouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "RentalHouses",
                type: "money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepositDueDate",
                table: "RentalHouses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DepositPaidDate",
                table: "RentalHouses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DepositReminderSent",
                table: "RentalHouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "RentalHouses",
                type: "money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemainingDueDate",
                table: "RentalHouses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RemainingPaidDate",
                table: "RentalHouses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RemainingReminderSent",
                table: "RentalHouses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "RentalHouses");

            migrationBuilder.DropColumn(
                name: "DepositDueDate",
                table: "RentalHouses");

            migrationBuilder.DropColumn(
                name: "DepositPaidDate",
                table: "RentalHouses");

            migrationBuilder.DropColumn(
                name: "DepositReminderSent",
                table: "RentalHouses");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "RentalHouses");

            migrationBuilder.DropColumn(
                name: "RemainingDueDate",
                table: "RentalHouses");

            migrationBuilder.DropColumn(
                name: "RemainingPaidDate",
                table: "RentalHouses");

            migrationBuilder.DropColumn(
                name: "RemainingReminderSent",
                table: "RentalHouses");
        }
    }
}
