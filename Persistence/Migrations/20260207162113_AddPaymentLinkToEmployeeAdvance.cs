using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentLinkToEmployeeAdvance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PARTY_ID",
                table: "EMPLOYEE_ADVANCE",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ADVANCE_ID",
                table: "EMPLOYEE_ADVANCE",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PAYMENT_ID",
                table: "EMPLOYEE_ADVANCE",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EMPLOYEE_ADVANCE_PAYMENT_ID",
                table: "EMPLOYEE_ADVANCE",
                column: "PAYMENT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLOYEE_ADVANCE_PAYMENT",
                table: "EMPLOYEE_ADVANCE",
                column: "PAYMENT_ID",
                principalTable: "PAYMENT",
                principalColumn: "PAYMENT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMPLOYEE_ADVANCE_PAYMENT",
                table: "EMPLOYEE_ADVANCE");

            migrationBuilder.DropIndex(
                name: "IX_EMPLOYEE_ADVANCE_PAYMENT_ID",
                table: "EMPLOYEE_ADVANCE");

            migrationBuilder.DropColumn(
                name: "PAYMENT_ID",
                table: "EMPLOYEE_ADVANCE");

            migrationBuilder.AlterColumn<string>(
                name: "PARTY_ID",
                table: "EMPLOYEE_ADVANCE",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ADVANCE_ID",
                table: "EMPLOYEE_ADVANCE",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldUnicode: false,
                oldMaxLength: 40)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
