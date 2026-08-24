using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RepointActorColumnsToAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PARTY_REL_USRLGN",
                table: "PARTY_RELATIONSHIP");

            migrationBuilder.DropForeignKey(
                name: "SLSOPP_USRLGN",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropForeignKey(
                name: "SLSOPPACT_USRLGN",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropForeignKey(
                name: "SLOPHI_USRLGN",
                table: "SALES_OPPORTUNITY_HISTORY");

            migrationBuilder.AlterColumn<string>(
                name: "MODIFIED_BY_USER_LOGIN",
                table: "SALES_OPPORTUNITY_HISTORY",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldUnicode: false,
                oldMaxLength: 250,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CREATED_BY_USER_LOGIN",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldUnicode: false,
                oldMaxLength: 250)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CREATED_BY_USER_LOGIN",
                table: "SALES_OPPORTUNITY",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldUnicode: false,
                oldMaxLength: 250,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CREATED_BY_USER_LOGIN",
                table: "PARTY_RELATIONSHIP",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldUnicode: false,
                oldMaxLength: 250,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "PARTY_REL_CREATED_BY_USER",
                table: "PARTY_RELATIONSHIP",
                column: "CREATED_BY_USER_LOGIN",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "SLSOPP_CREATED_BY_USER",
                table: "SALES_OPPORTUNITY",
                column: "CREATED_BY_USER_LOGIN",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "SLSOPPACT_CREATED_BY_USER",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "CREATED_BY_USER_LOGIN",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "SLOPHI_MODIFIED_BY_USER",
                table: "SALES_OPPORTUNITY_HISTORY",
                column: "MODIFIED_BY_USER_LOGIN",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PARTY_REL_CREATED_BY_USER",
                table: "PARTY_RELATIONSHIP");

            migrationBuilder.DropForeignKey(
                name: "SLSOPP_CREATED_BY_USER",
                table: "SALES_OPPORTUNITY");

            migrationBuilder.DropForeignKey(
                name: "SLSOPPACT_CREATED_BY_USER",
                table: "SALES_OPPORTUNITY_ACTION");

            migrationBuilder.DropForeignKey(
                name: "SLOPHI_MODIFIED_BY_USER",
                table: "SALES_OPPORTUNITY_HISTORY");

            migrationBuilder.AlterColumn<string>(
                name: "MODIFIED_BY_USER_LOGIN",
                table: "SALES_OPPORTUNITY_HISTORY",
                type: "varchar(250)",
                unicode: false,
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "SALES_OPPORTUNITY_ACTION",
                keyColumn: "CREATED_BY_USER_LOGIN",
                keyValue: null,
                column: "CREATED_BY_USER_LOGIN",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CREATED_BY_USER_LOGIN",
                table: "SALES_OPPORTUNITY_ACTION",
                type: "varchar(250)",
                unicode: false,
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CREATED_BY_USER_LOGIN",
                table: "SALES_OPPORTUNITY",
                type: "varchar(250)",
                unicode: false,
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CREATED_BY_USER_LOGIN",
                table: "PARTY_RELATIONSHIP",
                type: "varchar(250)",
                unicode: false,
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "PARTY_REL_USRLGN",
                table: "PARTY_RELATIONSHIP",
                column: "CREATED_BY_USER_LOGIN",
                principalTable: "USER_LOGIN",
                principalColumn: "USER_LOGIN_ID");

            migrationBuilder.AddForeignKey(
                name: "SLSOPP_USRLGN",
                table: "SALES_OPPORTUNITY",
                column: "CREATED_BY_USER_LOGIN",
                principalTable: "USER_LOGIN",
                principalColumn: "USER_LOGIN_ID");

            migrationBuilder.AddForeignKey(
                name: "SLSOPPACT_USRLGN",
                table: "SALES_OPPORTUNITY_ACTION",
                column: "CREATED_BY_USER_LOGIN",
                principalTable: "USER_LOGIN",
                principalColumn: "USER_LOGIN_ID");

            migrationBuilder.AddForeignKey(
                name: "SLOPHI_USRLGN",
                table: "SALES_OPPORTUNITY_HISTORY",
                column: "MODIFIED_BY_USER_LOGIN",
                principalTable: "USER_LOGIN",
                principalColumn: "USER_LOGIN_ID");
        }
    }
}
