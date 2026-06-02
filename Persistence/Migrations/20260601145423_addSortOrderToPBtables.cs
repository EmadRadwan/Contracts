using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addSortOrderToPBtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SORT_ORDER",
                table: "GL_SUB_CLASS_2",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SORT_ORDER",
                table: "GL_SUB_CLASS",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SORT_ORDER",
                table: "GL_SUB_ACCOUNT_COURSE_LABEL",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SORT_ORDER",
                table: "GL_REPORT",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SORT_ORDER",
                table: "GL_CLASS_COURSE",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SORT_ORDER",
                table: "GL_ACCOUNT_COURSE_LABEL",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "GL_SUB_CLASS_2");

            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "GL_SUB_CLASS");

            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "GL_SUB_ACCOUNT_COURSE_LABEL");

            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "GL_REPORT");

            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "GL_CLASS_COURSE");

            migrationBuilder.DropColumn(
                name: "SORT_ORDER",
                table: "GL_ACCOUNT_COURSE_LABEL");
        }
    }
}
