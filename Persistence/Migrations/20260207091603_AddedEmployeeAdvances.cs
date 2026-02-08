using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmployeeAdvances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EMPLOYEE_ADVANCE",
                columns: table => new
                {
                    ADVANCE_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PARTY_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ADVANCE_DATE = table.Column<DateTime>(type: "datetime", nullable: false),
                    AMOUNT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CURRENCY_UOM_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    INSTALLMENT_COUNT = table.Column<int>(type: "int", nullable: false),
                    INSTALLMENT_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    START_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    STATUS_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: false),
                    CREATED_TX_STAMP = table.Column<DateTime>(type: "datetime", nullable: false),
                    LAST_UPDATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: false),
                    LAST_UPDATED_TX_STAMP = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPLOYEE_ADVANCE", x => x.ADVANCE_ID);
                    table.ForeignKey(
                        name: "ADVANCE_PARTY_FK",
                        column: x => x.PARTY_ID,
                        principalTable: "PARTY",
                        principalColumn: "PARTY_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EMPLOYEE_ADVANCE_SCHEDULE",
                columns: table => new
                {
                    SCHEDULE_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ADVANCE_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    INSTALLMENT_NUM = table.Column<int>(type: "int", nullable: false),
                    DUE_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    SCHED_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DEDUCTED_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    REMAINING_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    STATUS_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PAYROL_INVOICE_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NOTES = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: false),
                    CREATED_TX_STAMP = table.Column<DateTime>(type: "datetime", nullable: false),
                    LAST_UPDATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: false),
                    LAST_UPDATED_TX_STAMP = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPLOYEE_ADVANCE_SCHEDULE", x => x.SCHEDULE_ID);
                    table.ForeignKey(
                        name: "ADV_SCH_ADVANCE_FK",
                        column: x => x.ADVANCE_ID,
                        principalTable: "EMPLOYEE_ADVANCE",
                        principalColumn: "ADVANCE_ID");
                    table.ForeignKey(
                        name: "ADV_SCH_INVOICE_FK",
                        column: x => x.PAYROL_INVOICE_ID,
                        principalTable: "INVOICE",
                        principalColumn: "INVOICE_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ADVANCE_DATE_IDX",
                table: "EMPLOYEE_ADVANCE",
                column: "ADVANCE_DATE");

            migrationBuilder.CreateIndex(
                name: "ADVANCE_PARTY_IDX",
                table: "EMPLOYEE_ADVANCE",
                column: "PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "ADVANCE_TXCRTS",
                table: "EMPLOYEE_ADVANCE",
                column: "CREATED_TX_STAMP");

            migrationBuilder.CreateIndex(
                name: "ADVANCE_TXSTMP",
                table: "EMPLOYEE_ADVANCE",
                column: "LAST_UPDATED_TX_STAMP");

            migrationBuilder.CreateIndex(
                name: "ADV_SCH_ADVANCE_IDX",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                column: "ADVANCE_ID");

            migrationBuilder.CreateIndex(
                name: "ADV_SCH_DUE_DATE_IDX",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                column: "DUE_DATE");

            migrationBuilder.CreateIndex(
                name: "ADV_SCH_INVOICE_IDX",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                column: "PAYROL_INVOICE_ID");

            migrationBuilder.CreateIndex(
                name: "ADV_SCH_TXCRTS",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                column: "CREATED_TX_STAMP");

            migrationBuilder.CreateIndex(
                name: "ADV_SCH_TXSTMP",
                table: "EMPLOYEE_ADVANCE_SCHEDULE",
                column: "LAST_UPDATED_TX_STAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EMPLOYEE_ADVANCE_SCHEDULE");

            migrationBuilder.DropTable(
                name: "EMPLOYEE_ADVANCE");
        }
    }
}
