using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactoredSalesOppActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IS_ANSWERED",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.UpdateData(
                table: "SALES_OPPORTUNITY_ACTION",
                keyColumn: "ACTION_TYPE_ID",
                keyValue: null,
                column: "ACTION_TYPE_ID",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ACTION_TYPE_ID",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IS_WON",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NEXT_ACTION_TYPE_ID",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_IS_WON",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "IS_WON");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_NEXT_ACTION",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "NEXT_ACTION_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_OPP_ACTION",
                table: "SALES_OPPORTUNITY_ACTION",
                columns: new[] { "SALES_OPPORTUNITY_ID", "ACTION_TYPE_ID" });

            migrationBuilder.AddForeignKey(
                name: "SLSOPPACT_NEXT_ACTION_TYP",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "NEXT_ACTION_TYPE_ID",
                principalTable: "ENUMERATION",
                principalColumn: "ENUM_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "SLSOPPACT_NEXT_ACTION_TYP",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropIndex(
                name: "SLSOPPACT_IS_WON",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropIndex(
                name: "SLSOPPACT_NEXT_ACTION",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropIndex(
                name: "SLSOPPACT_OPP_ACTION",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropColumn(
                name: "IS_WON",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropColumn(
                name: "NEXT_ACTION_TYPE_ID",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ANSWERED",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ACTION_TYPE_ID",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
