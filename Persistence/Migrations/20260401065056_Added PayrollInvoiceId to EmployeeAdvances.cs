using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedPayrollInvoiceIdtoEmployeeAdvances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PAYROLL_INVOICE_ID",
                table: "EMPLOYEE_ADVANCE",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EMPLOYEE_ADVANCE_PAYROLL_INVOICE_ID",
                table: "EMPLOYEE_ADVANCE",
                column: "PAYROLL_INVOICE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_ADV_PAYROLL_INVOICE",
                table: "EMPLOYEE_ADVANCE",
                column: "PAYROLL_INVOICE_ID",
                principalTable: "INVOICE",
                principalColumn: "INVOICE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_ADV_PAYROLL_INVOICE",
                table: "EMPLOYEE_ADVANCE");

            migrationBuilder.DropIndex(
                name: "IX_EMPLOYEE_ADVANCE_PAYROLL_INVOICE_ID",
                table: "EMPLOYEE_ADVANCE");

            migrationBuilder.DropColumn(
                name: "PAYROLL_INVOICE_ID",
                table: "EMPLOYEE_ADVANCE");
        }
    }
}
