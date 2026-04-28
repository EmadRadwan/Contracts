using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Adjusteddescriptionsizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "COMMENTS",
                table: "PAYMENT",
                type: "varchar(2500)",
                unicode: false,
                maxLength: 2500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1500)",
                oldUnicode: false,
                oldMaxLength: 1500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DESCRIPTION",
                table: "ACCTG_TRANS_ENTRY",
                type: "varchar(2500)",
                unicode: false,
                maxLength: 2500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DESCRIPTION",
                table: "ACCTG_TRANS",
                type: "varchar(1500)",
                unicode: false,
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "COMMENTS",
                table: "PAYMENT",
                type: "varchar(1500)",
                unicode: false,
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2500)",
                oldUnicode: false,
                oldMaxLength: 2500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DESCRIPTION",
                table: "ACCTG_TRANS_ENTRY",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2500)",
                oldUnicode: false,
                oldMaxLength: 2500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DESCRIPTION",
                table: "ACCTG_TRANS",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1500)",
                oldUnicode: false,
                oldMaxLength: 1500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
