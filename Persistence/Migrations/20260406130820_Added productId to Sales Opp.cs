using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedproductIdtoSalesOpp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PRODUCT_ID",
                table: "SALES_OPPORTUNITY",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WORK_EFFORT_ID",
                table: "SALES_OPPORTUNITY",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "SLSOPP_PRODUCT",
                table: "SALES_OPPORTUNITY",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "SLSOPP_WORKEFFORT",
                table: "SALES_OPPORTUNITY",
                column: "WORK_EFFORT_ID");

            migrationBuilder.AddForeignKey(
                name: "SLSOPP_TO_PRODUCT",
                table: "SALES_OPPORTUNITY",
                column: "PRODUCT_ID",
                principalTable: "PRODUCT",
                principalColumn: "PRODUCT_ID");

            migrationBuilder.AddForeignKey(
                name: "SLSOPP_TO_WORKEFFORT",
                table: "SALES_OPPORTUNITY",
                column: "WORK_EFFORT_ID",
                principalTable: "WORK_EFFORT",
                principalColumn: "WORK_EFFORT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "SLSOPP_TO_PRODUCT",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropForeignKey(
                name: "SLSOPP_TO_WORKEFFORT",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropIndex(
                name: "SLSOPP_PRODUCT",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropIndex(
                name: "SLSOPP_WORKEFFORT",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropColumn(
                name: "PRODUCT_ID",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropColumn(
                name: "WORK_EFFORT_ID",
                table: "SALES_OPPORTUNITY");
        }

    }
}
