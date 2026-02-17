using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Adjustedempladvancetables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "REMAINING_AMT",
                table: "EMPLOYEE_ADVANCE_SCHEDULE");

            migrationBuilder.DropColumn(
                name: "CURRENCY_UOM_ID",
                table: "EMPLOYEE_ADVANCE");

            migrationBuilder.DropColumn(
                name: "INSTALLMENT_AMT",
                table: "EMPLOYEE_ADVANCE");

            migrationBuilder.AlterColumn<string>(
                name: "STATUS_ID",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "START_DATE",
                table: "EMPLOYEE_ADVANCE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<int>(
                name: "INSTALLMENT_COUNT",
                table: "EMPLOYEE_ADVANCE",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AdvanceTypeId",
                table: "EMPLOYEE_ADVANCE",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvanceTypeId",
                table: "EMPLOYEE_ADVANCE");

            migrationBuilder.UpdateData(
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                keyColumn: "STATUS_ID",
                keyValue: null,
                column: "STATUS_ID",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "STATUS_ID",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "REMAINING_AMT",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "START_DATE",
                table: "EMPLOYEE_ADVANCE",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "INSTALLMENT_COUNT",
                table: "EMPLOYEE_ADVANCE",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CURRENCY_UOM_ID",
                table: "EMPLOYEE_ADVANCE",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "INSTALLMENT_AMT",
                table: "EMPLOYEE_ADVANCE",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
