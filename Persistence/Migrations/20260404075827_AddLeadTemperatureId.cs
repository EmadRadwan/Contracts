using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadTemperatureId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LEAD_TEMPRATURE_ID",
                table: "PARTY",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "PARTY_LEADTEMP",
                table: "PARTY",
                column: "LEAD_TEMPRATURE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "PARTY_LEADTEMP",
                table: "PARTY");

            migrationBuilder.DropColumn(
                name: "LEAD_TEMPRATURE_ID",
                table: "PARTY");
        }
    }
}
