using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserLoginToPartyRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CREATED_BY_USER_LOGIN",
                table: "PARTY_RELATIONSHIP",
                type: "varchar(250)",
                unicode: false,
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "PARTY_REL_USRLGN",
                table: "PARTY_RELATIONSHIP",
                column: "CREATED_BY_USER_LOGIN");

            migrationBuilder.AddForeignKey(
                name: "PARTY_REL_USRLGN",
                table: "PARTY_RELATIONSHIP",
                column: "CREATED_BY_USER_LOGIN",
                principalTable: "USER_LOGIN",
                principalColumn: "USER_LOGIN_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PARTY_REL_USRLGN",
                table: "PARTY_RELATIONSHIP");

            migrationBuilder.DropIndex(
                name: "PARTY_REL_USRLGN",
                table: "PARTY_RELATIONSHIP");

            migrationBuilder.DropColumn(
                name: "CREATED_BY_USER_LOGIN",
                table: "PARTY_RELATIONSHIP");
        }
    }
}
