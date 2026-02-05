using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdjustedPartyGlAccountPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PARTY_GL_ACCOUNT",
                table: "PARTY_GL_ACCOUNT");

            migrationBuilder.UpdateData(
                table: "PARTY_GL_ACCOUNT",
                keyColumn: "GL_ACCOUNT_ID",
                keyValue: null,
                column: "GL_ACCOUNT_ID",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "GL_ACCOUNT_ID",
                table: "PARTY_GL_ACCOUNT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PARTY_GL_ACCOUNT",
                table: "PARTY_GL_ACCOUNT",
                columns: new[] { "ORGANIZATION_PARTY_ID", "PARTY_ID", "ROLE_TYPE_ID", "GL_ACCOUNT_TYPE_ID", "GL_ACCOUNT_ID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PARTY_GL_ACCOUNT",
                table: "PARTY_GL_ACCOUNT");

            migrationBuilder.AlterColumn<string>(
                name: "GL_ACCOUNT_ID",
                table: "PARTY_GL_ACCOUNT",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PARTY_GL_ACCOUNT",
                table: "PARTY_GL_ACCOUNT",
                columns: new[] { "ORGANIZATION_PARTY_ID", "PARTY_ID", "ROLE_TYPE_ID", "GL_ACCOUNT_TYPE_ID" });
        }
    }
}
