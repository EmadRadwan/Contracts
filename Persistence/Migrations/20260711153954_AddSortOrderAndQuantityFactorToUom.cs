using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSortOrderAndQuantityFactorToUom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BASE_UOM_ID",
                table: "UOM",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "QUANTITY_FACTOR",
                table: "UOM",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SORT_ORDER",
                table: "UOM",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UOM_TO_BASE_UOM",
                table: "UOM",
                column: "BASE_UOM_ID");

            migrationBuilder.AddForeignKey(
                name: "UOM_TO_BASE_UOM",
                table: "UOM",
                column: "BASE_UOM_ID",
                principalTable: "UOM",
                principalColumn: "UOM_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "UOM_TO_BASE_UOM",
                table: "UOM");

            migrationBuilder.DropIndex(
                name: "UOM_TO_BASE_UOM",
                table: "UOM");

            migrationBuilder.DropColumn(
                name: "BASE_UOM_ID",
                table: "UOM");

            migrationBuilder.DropColumn(
                name: "QUANTITY_FACTOR",
                table: "UOM");

            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "UOM");
        }
    }
}
