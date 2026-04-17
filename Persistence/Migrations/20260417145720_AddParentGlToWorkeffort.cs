using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParentGlToWorkeffort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OPERATING_EXPENSE_GL_ACCOUNT_ID",
                table: "WORK_EFFORT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "WK_EFFRT_OP_EXP_GL",
                table: "WORK_EFFORT",
                column: "OPERATING_EXPENSE_GL_ACCOUNT_ID");

            migrationBuilder.AddForeignKey(
                name: "WK_EFFRT_OP_EXP_GL",
                table: "WORK_EFFORT",
                column: "OPERATING_EXPENSE_GL_ACCOUNT_ID",
                principalTable: "GL_ACCOUNT",
                principalColumn: "GL_ACCOUNT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "WK_EFFRT_OP_EXP_GL",
                table: "WORK_EFFORT");

            migrationBuilder.DropIndex(
                name: "WK_EFFRT_OP_EXP_GL",
                table: "WORK_EFFORT");

            migrationBuilder.DropColumn(
                name: "OPERATING_EXPENSE_GL_ACCOUNT_ID",
                table: "WORK_EFFORT");
        }
    }
}
