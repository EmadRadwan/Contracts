using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesCommissionSplitFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MANAGER2_AMOUNT",
                table: "SALES_COMMISSION",
                type: "decimal(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MANAGER2_NET_AMOUNT",
                table: "SALES_COMMISSION",
                type: "decimal(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MANAGER2_PARTY_ID",
                table: "SALES_COMMISSION",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "MANAGER2_PERCENT",
                table: "SALES_COMMISSION",
                type: "decimal(8,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SALES_REP2_AMOUNT",
                table: "SALES_COMMISSION",
                type: "decimal(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SALES_REP2_NET_AMOUNT",
                table: "SALES_COMMISSION",
                type: "decimal(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SALES_REP2_PARTY_ID",
                table: "SALES_COMMISSION",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "SALES_REP2_PERCENT",
                table: "SALES_COMMISSION",
                type: "decimal(8,4)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMMISSION_MANAGER2_PARTY_ID",
                table: "SALES_COMMISSION",
                column: "MANAGER2_PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMMISSION_SALES_REP2_PARTY_ID",
                table: "SALES_COMMISSION",
                column: "SALES_REP2_PARTY_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SALES_COMM_MANAGER2",
                table: "SALES_COMMISSION",
                column: "MANAGER2_PARTY_ID",
                principalTable: "PARTY",
                principalColumn: "PARTY_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SALES_COMM_SALES_REP2",
                table: "SALES_COMMISSION",
                column: "SALES_REP2_PARTY_ID",
                principalTable: "PARTY",
                principalColumn: "PARTY_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SALES_COMM_MANAGER2",
                table: "SALES_COMMISSION");

            migrationBuilder.DropForeignKey(
                name: "FK_SALES_COMM_SALES_REP2",
                table: "SALES_COMMISSION");

            migrationBuilder.DropIndex(
                name: "IX_SALES_COMMISSION_MANAGER2_PARTY_ID",
                table: "SALES_COMMISSION");

            migrationBuilder.DropIndex(
                name: "IX_SALES_COMMISSION_SALES_REP2_PARTY_ID",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "MANAGER2_AMOUNT",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "MANAGER2_NET_AMOUNT",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "MANAGER2_PARTY_ID",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "MANAGER2_PERCENT",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "SALES_REP2_AMOUNT",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "SALES_REP2_NET_AMOUNT",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "SALES_REP2_PARTY_ID",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "SALES_REP2_PERCENT",
                table: "SALES_COMMISSION");
        }
    }
}
