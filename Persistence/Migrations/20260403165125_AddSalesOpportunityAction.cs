using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOpportunityAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SALES_OPPORTUNITY_ACTION",
                columns: table => new
                {
                    SALES_OPPORTUNITY_ACTION_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SALES_OPPORTUNITY_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ACTION_TYPE_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IS_ANSWERED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ACTION_DATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    CANCEL_REASON_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    COMMENT = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_BY_USER_LOGIN = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: false),
                    LAST_UPDATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: false),
                    CREATED_TX_STAMP = table.Column<DateTime>(type: "datetime", nullable: true),
                    LAST_UPDATED_TX_STAMP = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALES_OPPORTUNITY_ACTION", x => x.SALES_OPPORTUNITY_ACTION_ID);
                    table.ForeignKey(
                        name: "SLSOPPACT_ACTION_TYP",
                        column: x => x.ACTION_TYPE_ID,
                        principalTable: "ENUMERATION",
                        principalColumn: "ENUM_ID");
                    table.ForeignKey(
                        name: "SLSOPPACT_CANCEL_RSN",
                        column: x => x.CANCEL_REASON_ID,
                        principalTable: "ENUMERATION",
                        principalColumn: "ENUM_ID");
                    table.ForeignKey(
                        name: "SLSOPPACT_SLSOPP",
                        column: x => x.SALES_OPPORTUNITY_ID,
                        principalTable: "SALES_OPPORTUNITY",
                        principalColumn: "SALES_OPPORTUNITY_ID");
                    table.ForeignKey(
                        name: "SLSOPPACT_USRLGN",
                        column: x => x.CREATED_BY_USER_LOGIN,
                        principalTable: "USER_LOGIN",
                        principalColumn: "USER_LOGIN_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_ACTION_TYP",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "ACTION_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_CANCEL_RSN",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "CANCEL_REASON_ID");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_CRTS",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "CREATED_STAMP");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_OPPID",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "SALES_OPPORTUNITY_ID");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_UPDST",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "LAST_UPDATED_STAMP");

            migrationBuilder.CreateIndex(
                name: "SLSOPPACT_USRLGN",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "CREATED_BY_USER_LOGIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SALES_OPPORTUNITY_ACTION");
        }
    }
}
