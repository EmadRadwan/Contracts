using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Convert_BusinessDates_To_DateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "THRU_DATE",
                table: "UOM_CONVERSION_DATEDS",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "FROM_DATE",
                table: "UOM_CONVERSION_DATEDS",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DATETIME_RECEIVED",
                table: "SHIPMENT_RECEIPT",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DUE_DATE",
                table: "SALES_REQUEST_INSTALLMENT",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "SALE_DATE",
                table: "SALES_REQUEST",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DATE_OF_FIRST_INSTALLMENT",
                table: "SALES_REQUEST",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EFFECTIVE_DATE",
                table: "PAYMENT",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ChequeDate",
                table: "PAYMENT",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ISSUED_DATE_TIME",
                table: "ITEM_ISSUANCE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PAID_DATE",
                table: "INVOICE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "INVOICE_DATE",
                table: "INVOICE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DUE_DATE",
                table: "INVOICE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TRANSACTION_DATE",
                table: "FIN_ACCOUNT_TRANS",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ENTRY_DATE",
                table: "FIN_ACCOUNT_TRANS",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DUE_DATE",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ADVANCE_DATE",
                table: "EMPLOYEE_ADVANCE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "THRU_DATE",
                table: "UOM_CONVERSION_DATEDS",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FROM_DATE",
                table: "UOM_CONVERSION_DATEDS",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DATETIME_RECEIVED",
                table: "SHIPMENT_RECEIPT",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DUE_DATE",
                table: "SALES_REQUEST_INSTALLMENT",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SALE_DATE",
                table: "SALES_REQUEST",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DATE_OF_FIRST_INSTALLMENT",
                table: "SALES_REQUEST",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EFFECTIVE_DATE",
                table: "PAYMENT",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ChequeDate",
                table: "PAYMENT",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ISSUED_DATE_TIME",
                table: "ITEM_ISSUANCE",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PAID_DATE",
                table: "INVOICE",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "INVOICE_DATE",
                table: "INVOICE",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DUE_DATE",
                table: "INVOICE",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TRANSACTION_DATE",
                table: "FIN_ACCOUNT_TRANS",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ENTRY_DATE",
                table: "FIN_ACCOUNT_TRANS",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DUE_DATE",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ADVANCE_DATE",
                table: "EMPLOYEE_ADVANCE",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
