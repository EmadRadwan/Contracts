using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesCommissionAndProjectCommissionRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROJECT_COMMISSION_RATE",
                columns: table => new
                {
                    PROJECT_COMMISSION_RATE_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PROJECT_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SALE_TYPE_ID = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SALES_REP_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    MANAGER_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    EXTERNAL_COMPANY_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    EXTERNAL_SALES_REP_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    EXTERNAL_MANAGER_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    LAST_UPDATED_STAMP = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CREATED_STAMP = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECT_COMMISSION_RATE", x => x.PROJECT_COMMISSION_RATE_ID);
                    table.ForeignKey(
                        name: "FK_PROJ_COMM_RATE_PROJECT",
                        column: x => x.PROJECT_ID,
                        principalTable: "WORK_EFFORT",
                        principalColumn: "WORK_EFFORT_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SALES_COMMISSION",
                columns: table => new
                {
                    SALES_COMMISSION_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SALES_REQUEST_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SALE_TYPE_ID = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    STATUS_ID = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    COMMISSION_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PROJECT_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SALE_PRICE = table.Column<decimal>(type: "decimal(20,2)", nullable: false),
                    COLLECTED_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    SALES_REP_PARTY_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SALES_REP_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    SALES_REP_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    SALES_REP_NET_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    MANAGER_PARTY_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MANAGER_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    MANAGER_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    MANAGER_NET_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    EXT_COMPANY_PARTY_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXT_COMPANY_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    EXT_COMPANY_GROSS_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    EXT_COMPANY_NET_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    EXT_SALES_REP_PARTY_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXT_SALES_REP_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    EXT_SALES_REP_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    EXT_SALES_REP_NET_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    EXT_MANAGER_PARTY_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXT_MANAGER_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    EXT_MANAGER_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    EXT_MANAGER_NET_AMOUNT = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    HAS_VAT_EXEMPTION = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HAS_WITHHOLDING_TAX_EXEMPTION = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    VAT_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    WITHHOLDING_TAX_PERCENT = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    NOTES = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LAST_UPDATED_STAMP = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CREATED_STAMP = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALES_COMMISSION", x => x.SALES_COMMISSION_ID);
                    table.ForeignKey(
                        name: "FK_SALES_COMM_EXT_COMPANY",
                        column: x => x.EXT_COMPANY_PARTY_ID,
                        principalTable: "PARTY",
                        principalColumn: "PARTY_ID");
                    table.ForeignKey(
                        name: "FK_SALES_COMM_EXT_MGR",
                        column: x => x.EXT_MANAGER_PARTY_ID,
                        principalTable: "PARTY",
                        principalColumn: "PARTY_ID");
                    table.ForeignKey(
                        name: "FK_SALES_COMM_EXT_SR",
                        column: x => x.EXT_SALES_REP_PARTY_ID,
                        principalTable: "PARTY",
                        principalColumn: "PARTY_ID");
                    table.ForeignKey(
                        name: "FK_SALES_COMM_MANAGER",
                        column: x => x.MANAGER_PARTY_ID,
                        principalTable: "PARTY",
                        principalColumn: "PARTY_ID");
                    table.ForeignKey(
                        name: "FK_SALES_COMM_PROJECT",
                        column: x => x.PROJECT_ID,
                        principalTable: "WORK_EFFORT",
                        principalColumn: "WORK_EFFORT_ID");
                    table.ForeignKey(
                        name: "FK_SALES_COMM_SALES_REP",
                        column: x => x.SALES_REP_PARTY_ID,
                        principalTable: "PARTY",
                        principalColumn: "PARTY_ID");
                    table.ForeignKey(
                        name: "FK_SALES_COMM_SR",
                        column: x => x.SALES_REQUEST_ID,
                        principalTable: "SALES_REQUEST",
                        principalColumn: "SALES_REQUEST_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PROJ_COMM_RATE_PROJ_TYPE",
                table: "PROJECT_COMMISSION_RATE",
                columns: new[] { "PROJECT_ID", "SALE_TYPE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMM_SR_ID",
                table: "SALES_COMMISSION",
                column: "SALES_REQUEST_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMMISSION_EXT_COMPANY_PARTY_ID",
                table: "SALES_COMMISSION",
                column: "EXT_COMPANY_PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMMISSION_EXT_MANAGER_PARTY_ID",
                table: "SALES_COMMISSION",
                column: "EXT_MANAGER_PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMMISSION_EXT_SALES_REP_PARTY_ID",
                table: "SALES_COMMISSION",
                column: "EXT_SALES_REP_PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMMISSION_MANAGER_PARTY_ID",
                table: "SALES_COMMISSION",
                column: "MANAGER_PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMMISSION_PROJECT_ID",
                table: "SALES_COMMISSION",
                column: "PROJECT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_COMMISSION_SALES_REP_PARTY_ID",
                table: "SALES_COMMISSION",
                column: "SALES_REP_PARTY_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROJECT_COMMISSION_RATE");

            migrationBuilder.DropTable(
                name: "SALES_COMMISSION");
        }
    }
}
