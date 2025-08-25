using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseNet9.Migrations
{
    /// <inheritdoc />
    public partial class DistanceDistanceItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Distances",
                columns: table => new
                {
                    DistanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HouseId = table.Column<int>(type: "int", nullable: true),
                    ImageFileID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distances", x => x.DistanceID);
                    table.ForeignKey(
                        name: "FK_Distances_Houses_HouseId",
                        column: x => x.HouseId,
                        principalTable: "Houses",
                        principalColumn: "HouseId");
                    table.ForeignKey(
                        name: "FK_Distances_MyFiles_ImageFileID",
                        column: x => x.ImageFileID,
                        principalTable: "MyFiles",
                        principalColumn: "FileID");
                });

            migrationBuilder.CreateTable(
                name: "DistanceItems",
                columns: table => new
                {
                    DistanceItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistanceInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistanceID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistanceItems", x => x.DistanceItemId);
                    table.ForeignKey(
                        name: "FK_DistanceItems_Distances_DistanceID",
                        column: x => x.DistanceID,
                        principalTable: "Distances",
                        principalColumn: "DistanceID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistanceItems_DistanceID",
                table: "DistanceItems",
                column: "DistanceID");

            migrationBuilder.CreateIndex(
                name: "IX_Distances_HouseId",
                table: "Distances",
                column: "HouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Distances_ImageFileID",
                table: "Distances",
                column: "ImageFileID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistanceItems");

            migrationBuilder.DropTable(
                name: "Distances");
        }
    }
}
