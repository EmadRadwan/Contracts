using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdjustedFinTransRelationToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PAYMENT_FACTX",
                table: "PAYMENT");

            migrationBuilder.RenameColumn(
                name: "FIN_ACCOUNT_TRANS_ID",
                table: "PAYMENT",
                newName: "FinAccountTransId");

            migrationBuilder.RenameIndex(
                name: "PAYMENT_FACTX",
                table: "PAYMENT",
                newName: "IX_PAYMENT_FinAccountTransId");

            migrationBuilder.AddForeignKey(
                name: "FK_PAYMENT_FIN_ACCOUNT_TRANS_FinAccountTransId",
                table: "PAYMENT",
                column: "FinAccountTransId",
                principalTable: "FIN_ACCOUNT_TRANS",
                principalColumn: "FIN_ACCOUNT_TRANS_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PAYMENT_FIN_ACCOUNT_TRANS_FinAccountTransId",
                table: "PAYMENT");

            migrationBuilder.RenameColumn(
                name: "FinAccountTransId",
                table: "PAYMENT",
                newName: "FIN_ACCOUNT_TRANS_ID");

            migrationBuilder.RenameIndex(
                name: "IX_PAYMENT_FinAccountTransId",
                table: "PAYMENT",
                newName: "PAYMENT_FACTX");

            migrationBuilder.AddForeignKey(
                name: "PAYMENT_FACTX",
                table: "PAYMENT",
                column: "FIN_ACCOUNT_TRANS_ID",
                principalTable: "FIN_ACCOUNT_TRANS",
                principalColumn: "FIN_ACCOUNT_TRANS_ID");
        }
    }
}
