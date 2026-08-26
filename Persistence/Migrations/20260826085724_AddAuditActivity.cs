using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AUDIT_ACTIVITY",
                columns: table => new
                {
                    ACTIVITY_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CORRELATION_ID = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    USER_NAME = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    USER_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    REQUEST_NAME = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    REQUEST_PATH = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HTTP_METHOD = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CLIENT_IP_ADDRESS = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    REQUEST_JSON = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IS_SUCCESS = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXCEPTION_TYPE = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DURATION_MS = table.Column<int>(type: "int", nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CREATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: true),
                    LAST_UPDATED_STAMP = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDIT_ACTIVITY", x => x.ACTIVITY_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ENTITY_AUDIT_LOG_CORRELATION",
                table: "ENTITY_AUDIT_LOG",
                column: "CHANGED_SESSION_INFO");

            migrationBuilder.CreateIndex(
                name: "ENTITY_AUDIT_LOG_RECORD",
                table: "ENTITY_AUDIT_LOG",
                columns: new[] { "CHANGED_ENTITY_NAME", "PK_COMBINED_VALUE_TEXT", "CHANGED_DATE" });

            migrationBuilder.CreateIndex(
                name: "AUDIT_ACTIVITY_CORRELATION",
                table: "AUDIT_ACTIVITY",
                column: "CORRELATION_ID");

            migrationBuilder.CreateIndex(
                name: "AUDIT_ACTIVITY_REQUEST",
                table: "AUDIT_ACTIVITY",
                columns: new[] { "REQUEST_NAME", "STARTED_AT" });

            migrationBuilder.CreateIndex(
                name: "AUDIT_ACTIVITY_STARTED",
                table: "AUDIT_ACTIVITY",
                column: "STARTED_AT");

            migrationBuilder.CreateIndex(
                name: "AUDIT_ACTIVITY_USER",
                table: "AUDIT_ACTIVITY",
                columns: new[] { "USER_ID", "STARTED_AT" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AUDIT_ACTIVITY");

            migrationBuilder.DropIndex(
                name: "ENTITY_AUDIT_LOG_CORRELATION",
                table: "ENTITY_AUDIT_LOG");

            migrationBuilder.DropIndex(
                name: "ENTITY_AUDIT_LOG_RECORD",
                table: "ENTITY_AUDIT_LOG");
        }
    }
}
