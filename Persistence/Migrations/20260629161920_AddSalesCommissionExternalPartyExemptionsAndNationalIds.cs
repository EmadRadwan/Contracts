using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesCommissionExternalPartyExemptionsAndNationalIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EXT_MANAGER_NATIONAL_ID",
                table: "SALES_COMMISSION",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EXT_SALES_REP_NATIONAL_ID",
                table: "SALES_COMMISSION",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "HAS_EXT_MANAGER_WHT_EXEMPTION",
                table: "SALES_COMMISSION",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HAS_EXT_SALES_REP_WHT_EXEMPTION",
                table: "SALES_COMMISSION",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EXT_MANAGER_NATIONAL_ID",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "EXT_SALES_REP_NATIONAL_ID",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "HAS_EXT_MANAGER_WHT_EXEMPTION",
                table: "SALES_COMMISSION");

            migrationBuilder.DropColumn(
                name: "HAS_EXT_SALES_REP_WHT_EXEMPTION",
                table: "SALES_COMMISSION");
        }
    }
}
