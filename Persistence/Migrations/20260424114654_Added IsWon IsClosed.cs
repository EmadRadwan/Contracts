using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsWonIsClosed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "SLSOPPACT_IS_WON",
                table: "SALES_OPPORTUNITY_ACTION",
                newName: "SLSOPPACT_IS_WON1");

            migrationBuilder.AddColumn<bool>(
                name: "IS_CLOSED",
                table: "SALES_OPPORTUNITY",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IS_WON",
                table: "SALES_OPPORTUNITY",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_IS_CLOSED",
                table: "SALES_OPPORTUNITY",
                column: "IS_CLOSED");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_IS_WON",
                table: "SALES_OPPORTUNITY",
                column: "IS_WON");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "SLSOPPACT_IS_CLOSED",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropIndex(
                name: "SLSOPPACT_IS_WON",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropColumn(
                name: "IS_CLOSED",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropColumn(
                name: "IS_WON",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.RenameIndex(
                name: "SLSOPPACT_IS_WON1",
                table: "SALES_OPPORTUNITY_ACTION",
                newName: "SLSOPPACT_IS_WON");
        }
    }
}
