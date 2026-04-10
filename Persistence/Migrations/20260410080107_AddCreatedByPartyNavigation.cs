using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByPartyNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CREATED_BY_PARTY_ID",
                table: "PAYMENT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "PAYMENT_CRTD_PTY",
                table: "PAYMENT",
                column: "CREATED_BY_PARTY_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PAYMENT_CREATED_BY_PARTY",
                table: "PAYMENT",
                column: "CREATED_BY_PARTY_ID",
                principalTable: "PARTY",
                principalColumn: "PARTY_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PAYMENT_CREATED_BY_PARTY",
                table: "PAYMENT");

            migrationBuilder.DropIndex(
                name: "PAYMENT_CRTD_PTY",
                table: "PAYMENT");

            migrationBuilder.DropColumn(
                name: "CREATED_BY_PARTY_ID",
                table: "PAYMENT");
        }
    }
}
