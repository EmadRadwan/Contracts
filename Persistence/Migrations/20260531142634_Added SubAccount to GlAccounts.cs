using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedSubAccounttoGlAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GL_SUB_ACCOUNT_COURSE_LABEL_ID",
                table: "GL_ACCOUNT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GL_SUB_ACCOUNT_COURSE_LABEL",
                columns: table => new
                {
                    GL_SUB_ACCOUNT_COURSE_LABEL_ID = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION_ARABIC = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GL_SUB_ACCOUNT_COURSE_LABEL", x => x.GL_SUB_ACCOUNT_COURSE_LABEL_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GL_ACCOUNT_GL_SUB_ACCOUNT_COURSE_LABEL_ID",
                table: "GL_ACCOUNT",
                column: "GL_SUB_ACCOUNT_COURSE_LABEL_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GL_ACCOUNT_GL_SUB_ACCOUNT_COURSE_LABEL_GL_SUB_ACCOUNT_COURSE~",
                table: "GL_ACCOUNT",
                column: "GL_SUB_ACCOUNT_COURSE_LABEL_ID",
                principalTable: "GL_SUB_ACCOUNT_COURSE_LABEL",
                principalColumn: "GL_SUB_ACCOUNT_COURSE_LABEL_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GL_ACCOUNT_GL_SUB_ACCOUNT_COURSE_LABEL_GL_SUB_ACCOUNT_COURSE~",
                table: "GL_ACCOUNT");

            migrationBuilder.DropTable(
                name: "GL_SUB_ACCOUNT_COURSE_LABEL");

            migrationBuilder.DropIndex(
                name: "IX_GL_ACCOUNT_GL_SUB_ACCOUNT_COURSE_LABEL_ID",
                table: "GL_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "GL_SUB_ACCOUNT_COURSE_LABEL_ID",
                table: "GL_ACCOUNT");
        }
    }
}
