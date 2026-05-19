using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_Department_And_Attendance_To_Party : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ATTENDANCE_STARTS_AT",
                table: "PARTY",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DEPARTMENT_PARTY_ID",
                table: "PARTY",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FINGER_PRINT_ATTENDANCE_ID",
                table: "PARTY",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PARTY_DEPARTMENT_PARTY_ID",
                table: "PARTY",
                column: "DEPARTMENT_PARTY_ID");

            migrationBuilder.AddForeignKey(
                name: "PARTY_DEPARTMENT_FK",
                table: "PARTY",
                column: "DEPARTMENT_PARTY_ID",
                principalTable: "PARTY",
                principalColumn: "PARTY_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PARTY_DEPARTMENT_FK",
                table: "PARTY");

            migrationBuilder.DropIndex(
                name: "IX_PARTY_DEPARTMENT_PARTY_ID",
                table: "PARTY");

            migrationBuilder.DropColumn(
                name: "ATTENDANCE_STARTS_AT",
                table: "PARTY");

            migrationBuilder.DropColumn(
                name: "DEPARTMENT_PARTY_ID",
                table: "PARTY");

            migrationBuilder.DropColumn(
                name: "FINGER_PRINT_ATTENDANCE_ID",
                table: "PARTY");
        }
    }
}
