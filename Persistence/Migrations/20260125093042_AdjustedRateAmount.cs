using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdjustedRateAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RATE_AMOUNT",
                table: "RATE_AMOUNT",
                newName: "AMOUNT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AMOUNT",
                table: "RATE_AMOUNT",
                newName: "RATE_AMOUNT");
        }
    }
}
