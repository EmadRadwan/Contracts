using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFinAccountTransIdFromPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FIN_ACT_TX_PMT",
                table: "FIN_ACCOUNT_TRANS");

            migrationBuilder.DropForeignKey(
                name: "FK_PAYMENT_FIN_ACCOUNT_TRANS_FinAccountTransId",
                table: "PAYMENT");

            migrationBuilder.DropIndex(
                name: "IX_PAYMENT_FinAccountTransId",
                table: "PAYMENT");

            migrationBuilder.DropColumn(
                name: "FinAccountTransId",
                table: "PAYMENT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinAccountTransId",
                table: "PAYMENT",
                type: "varchar(36)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_FinAccountTransId",
                table: "PAYMENT",
                column: "FinAccountTransId");

            migrationBuilder.AddForeignKey(
                name: "FIN_ACT_TX_PMT",
                table: "FIN_ACCOUNT_TRANS",
                column: "PAYMENT_ID",
                principalTable: "PAYMENT",
                principalColumn: "PAYMENT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PAYMENT_FIN_ACCOUNT_TRANS_FinAccountTransId",
                table: "PAYMENT",
                column: "FinAccountTransId",
                principalTable: "FIN_ACCOUNT_TRANS",
                principalColumn: "FIN_ACCOUNT_TRANS_ID");
        }
    }
}
