using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExtCompanyTaxInvoiceRaised : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EXT_COMPANY_TAX_INVOICE_RAISED",
                table: "SALES_COMMISSION",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EXT_COMPANY_TAX_INVOICE_RAISED",
                table: "SALES_COMMISSION");
        }
    }
}
