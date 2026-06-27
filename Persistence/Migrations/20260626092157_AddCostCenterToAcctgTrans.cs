using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCostCenterToAcctgTrans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "COST_CENTER_ID",
                table: "ACCTG_TRANS",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ACCTG_TRANS_COST_CENTER_ID",
                table: "ACCTG_TRANS",
                column: "COST_CENTER_ID");

            migrationBuilder.AddForeignKey(
                name: "ACCTTX_COST_CENTER",
                table: "ACCTG_TRANS",
                column: "COST_CENTER_ID",
                principalTable: "COST_CENTER",
                principalColumn: "COST_CENTER_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "ACCTTX_COST_CENTER",
                table: "ACCTG_TRANS");

            migrationBuilder.DropIndex(
                name: "IX_ACCTG_TRANS_COST_CENTER_ID",
                table: "ACCTG_TRANS");

            migrationBuilder.DropColumn(
                name: "COST_CENTER_ID",
                table: "ACCTG_TRANS");
        }
    }
}
