using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedpowerBitables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GL_ACCOUNT_COURSE_LABEL_ID",
                table: "GL_ACCOUNT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GL_CLASS_COURSE_ID",
                table: "GL_ACCOUNT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GL_REPORT_ID",
                table: "GL_ACCOUNT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GL_SUB_CLASS_2_ID",
                table: "GL_ACCOUNT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GL_SUB_CLASS_ID",
                table: "GL_ACCOUNT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GL_ACCOUNT_COURSE_LABEL",
                columns: table => new
                {
                    GL_ACCOUNT_COURSE_LABEL_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION_ARABIC = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SIGN_MULTIPLIER = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_ACCOUNT_COURSE_LABEL", x => x.GL_ACCOUNT_COURSE_LABEL_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GL_CLASS_COURSE",
                columns: table => new
                {
                    GL_CLASS_COURSE_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION_ARABIC = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_CLASS_COURSE", x => x.GL_CLASS_COURSE_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GL_REPORT",
                columns: table => new
                {
                    GL_REPORT_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION_ARABIC = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_REPORT", x => x.GL_REPORT_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GL_SUB_CLASS",
                columns: table => new
                {
                    GL_SUB_CLASS_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION_ARABIC = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_SUB_CLASS", x => x.GL_SUB_CLASS_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GL_SUB_CLASS_2",
                columns: table => new
                {
                    GL_SUB_CLASS_2_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION_ARABIC = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_SUB_CLASS_2", x => x.GL_SUB_CLASS_2_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_GL_ACCOUNT_COURSE_LABEL_ID",
                table: "GL_ACCOUNT",
                column: "GL_ACCOUNT_COURSE_LABEL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_GL_CLASS_COURSE_ID",
                table: "GL_ACCOUNT",
                column: "GL_CLASS_COURSE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_GL_REPORT_ID",
                table: "GL_ACCOUNT",
                column: "GL_REPORT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_GL_SUB_CLASS_2_ID",
                table: "GL_ACCOUNT",
                column: "GL_SUB_CLASS_2_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_GL_SUB_CLASS_ID",
                table: "GL_ACCOUNT",
                column: "GL_SUB_CLASS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GL_CLASS_COURSE",
                table: "GL_ACCOUNT",
                column: "GL_CLASS_COURSE_ID",
                principalTable: "GL_CLASS_COURSE",
                principalColumn: "GL_CLASS_COURSE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GL_COURSE_LABEL",
                table: "GL_ACCOUNT",
                column: "GL_ACCOUNT_COURSE_LABEL_ID",
                principalTable: "GL_ACCOUNT_COURSE_LABEL",
                principalColumn: "GL_ACCOUNT_COURSE_LABEL_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GL_REPORT",
                table: "GL_ACCOUNT",
                column: "GL_REPORT_ID",
                principalTable: "GL_REPORT",
                principalColumn: "GL_REPORT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GL_SUB_CLASS",
                table: "GL_ACCOUNT",
                column: "GL_SUB_CLASS_ID",
                principalTable: "GL_SUB_CLASS",
                principalColumn: "GL_SUB_CLASS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GL_SUB_CLASS_2",
                table: "GL_ACCOUNT",
                column: "GL_SUB_CLASS_2_ID",
                principalTable: "GL_SUB_CLASS_2",
                principalColumn: "GL_SUB_CLASS_2_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GL_CLASS_COURSE",
                table: "GL_ACCOUNT");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_COURSE_LABEL",
                table: "GL_ACCOUNT");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_REPORT",
                table: "GL_ACCOUNT");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_SUB_CLASS",
                table: "GL_ACCOUNT");

            migrationBuilder.DropForeignKey(
                name: "FK_GL_SUB_CLASS_2",
                table: "GL_ACCOUNT");

            migrationBuilder.DropTable(
                name: "GL_ACCOUNT_COURSE_LABEL");

            migrationBuilder.DropTable(
                name: "GL_CLASS_COURSE");

            migrationBuilder.DropTable(
                name: "GL_REPORT");

            migrationBuilder.DropTable(
                name: "GL_SUB_CLASS");

            migrationBuilder.DropTable(
                name: "GL_SUB_CLASS_2");

            migrationBuilder.DropIndex(
                name: "IX_GL_ACCOUNT_GL_ACCOUNT_COURSE_LABEL_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropIndex(
                name: "IX_GL_ACCOUNT_GL_CLASS_COURSE_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropIndex(
                name: "IX_GL_ACCOUNT_GL_REPORT_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropIndex(
                name: "IX_GL_ACCOUNT_GL_SUB_CLASS_2_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropIndex(
                name: "IX_GL_ACCOUNT_GL_SUB_CLASS_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "GL_ACCOUNT_COURSE_LABEL_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "GL_CLASS_COURSE_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "GL_REPORT_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "GL_SUB_CLASS_2_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "GL_SUB_CLASS_ID",
                table: "GL_ACCOUNT");
        }
    }
}
