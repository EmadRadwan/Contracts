using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedDESCRIPTIONArabic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION_ARABIC",
                table: "SALES_OPPORTUNITY_STAGE",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION_ARABIC",
                table: "DATA_SOURCE",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DESCRIPTION_ARABIC",
                table: "SALES_OPPORTUNITY_STAGE");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION_ARABIC",
                table: "DATA_SOURCE");
        }
    }
}
