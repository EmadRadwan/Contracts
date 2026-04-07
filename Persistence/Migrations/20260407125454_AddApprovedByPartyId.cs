using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovedByPartyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "APPROVED_BY_PARTY_ID",
                table: "PAYMENT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "PAYMENT_APPR_PTY",
                table: "PAYMENT",
                column: "APPROVED_BY_PARTY_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PAYMENT_APPROVED_BY_PARTY",
                table: "PAYMENT",
                column: "APPROVED_BY_PARTY_ID",
                principalTable: "PARTY",
                principalColumn: "PARTY_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PAYMENT_APPROVED_BY_PARTY",
                table: "PAYMENT");

            migrationBuilder.DropIndex(
                name: "PAYMENT_APPR_PTY",
                table: "PAYMENT");

            migrationBuilder.DropColumn(
                name: "APPROVED_BY_PARTY_ID",
                table: "PAYMENT");
        }
    }
}
