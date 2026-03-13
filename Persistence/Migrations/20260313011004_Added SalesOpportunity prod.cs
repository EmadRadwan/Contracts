using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedSalesOpportunityprod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SALES_OPPORTUNITY_PRODUCT",
                columns: table => new
                {
                    SALES_OPPORTUNITY_PRODUCT_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SALES_OPPORTUNITY_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PRODUCT_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WORK_EFFORT_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QUANTITY = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 1m),
                    NOTES = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FROM_DATE = table.Column<DateTime>(type: "datetime", nullable: false),
                    THRU_DATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    CREATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: true),
                    LAST_UPDATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: true),
                    CREATED_BY_USER_LOGIN = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LAST_MODIFIED_BY_USER_LOGIN = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALES_OPPORTUNITY_PRODUCT", x => x.SALES_OPPORTUNITY_PRODUCT_ID);
                    table.ForeignKey(
                        name: "SLSOPPRD_PROD",
                        column: x => x.PRODUCT_ID,
                        principalTable: "PRODUCT",
                        principalColumn: "PRODUCT_ID");
                    table.ForeignKey(
                        name: "SLSOPPRD_SLSOPP",
                        column: x => x.SALES_OPPORTUNITY_ID,
                        principalTable: "SALES_OPPORTUNITY",
                        principalColumn: "SALES_OPPORTUNITY_ID");
                    table.ForeignKey(
                        name: "SLSOPPRD_WKEFF",
                        column: x => x.WORK_EFFORT_ID,
                        principalTable: "WORK_EFFORT",
                        principalColumn: "WORK_EFFORT_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "SLSOPPRD_OPPID",
                table: "SALES_OPPORTUNITY_PRODUCT",
                column: "SALES_OPPORTUNITY_ID");

            migrationBuilder.CreateIndex(
                name: "SLSOPPRD_PRODID",
                table: "SALES_OPPORTUNITY_PRODUCT",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "SLSOPPRD_WKEFFID",
                table: "SALES_OPPORTUNITY_PRODUCT",
                column: "WORK_EFFORT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SALES_OPPORTUNITY_PRODUCT");
        }
    }
}
