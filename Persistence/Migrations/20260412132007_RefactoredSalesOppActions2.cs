using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactoredSalesOppActions2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MEETING_LOCATION_ID",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MEETING_TYPE_ID",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NOTE",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_MEETING_LOC",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "MEETING_LOCATION_ID");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_MEETING_TYP",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "MEETING_TYPE_ID");

            migrationBuilder.AddForeignKey(
                name: "SLSOPPACT_MEETING_LOC",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "MEETING_LOCATION_ID",
                principalTable: "ENUMERATION",
                principalColumn: "ENUM_ID");

            migrationBuilder.AddForeignKey(
                name: "SLSOPPACT_MEETING_TYP",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "MEETING_TYPE_ID",
                principalTable: "ENUMERATION",
                principalColumn: "ENUM_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "SLSOPPACT_MEETING_LOC",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropForeignKey(
                name: "SLSOPPACT_MEETING_TYP",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropIndex(
                name: "SLSOPPACT_MEETING_LOC",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropIndex(
                name: "SLSOPPACT_MEETING_TYP",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropColumn(
                name: "MEETING_LOCATION_ID",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropColumn(
                name: "MEETING_TYPE_ID",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropColumn(
                name: "NOTE",
                table: "SALES_OPPORTUNITY_ACTION");
        }
    }
}
