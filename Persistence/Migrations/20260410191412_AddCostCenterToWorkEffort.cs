using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCostCenterToWorkEffort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "INSURANCE",
                table: "WORK_EFFORT",
                newName: "Insurance");

            migrationBuilder.RenameColumn(
                name: "DISCOUNT",
                table: "WORK_EFFORT",
                newName: "Discount");

            migrationBuilder.RenameColumn(
                name: "DEDUCTIONS",
                table: "WORK_EFFORT",
                newName: "Deductions");

            migrationBuilder.RenameColumn(
                name: "ADDITIONAL_INSURANCE",
                table: "WORK_EFFORT",
                newName: "AdditionalInsurance");

            migrationBuilder.AlterColumn<decimal>(
                name: "Insurance",
                table: "WORK_EFFORT",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Discount",
                table: "WORK_EFFORT",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Deductions",
                table: "WORK_EFFORT",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalInsurance",
                table: "WORK_EFFORT",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "COST_CENTER_ID",
                table: "WORK_EFFORT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "WK_EFFRT_COST_CENTER",
                table: "WORK_EFFORT",
                column: "COST_CENTER_ID");

            migrationBuilder.AddForeignKey(
                name: "WK_EFFRT_COST_CENTER",
                table: "WORK_EFFORT",
                column: "COST_CENTER_ID",
                principalTable: "COST_CENTER",
                principalColumn: "COST_CENTER_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "WK_EFFRT_COST_CENTER",
                table: "WORK_EFFORT");

            migrationBuilder.DropIndex(
                name: "WK_EFFRT_COST_CENTER",
                table: "WORK_EFFORT");

            migrationBuilder.DropColumn(
                name: "COST_CENTER_ID",
                table: "WORK_EFFORT");

            migrationBuilder.RenameColumn(
                name: "Insurance",
                table: "WORK_EFFORT",
                newName: "INSURANCE");

            migrationBuilder.RenameColumn(
                name: "Discount",
                table: "WORK_EFFORT",
                newName: "DISCOUNT");

            migrationBuilder.RenameColumn(
                name: "Deductions",
                table: "WORK_EFFORT",
                newName: "DEDUCTIONS");

            migrationBuilder.RenameColumn(
                name: "AdditionalInsurance",
                table: "WORK_EFFORT",
                newName: "ADDITIONAL_INSURANCE");

            migrationBuilder.AlterColumn<decimal>(
                name: "INSURANCE",
                table: "WORK_EFFORT",
                type: "decimal(18,3)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DISCOUNT",
                table: "WORK_EFFORT",
                type: "decimal(18,3)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DEDUCTIONS",
                table: "WORK_EFFORT",
                type: "decimal(18,3)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ADDITIONAL_INSURANCE",
                table: "WORK_EFFORT",
                type: "decimal(18,3)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);
        }
    }
}
