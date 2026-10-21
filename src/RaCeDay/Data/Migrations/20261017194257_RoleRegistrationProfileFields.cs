using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaCeDay.Data.Migrations
{
    /// <inheritdoc />
    public partial class RoleRegistrationProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganiserName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_DateOfBirth",
                table: "AspNetUsers",
                sql: "[DateOfBirth] IS NULL OR [DateOfBirth] >= '1900-01-01'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_User_DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrganiserName",
                table: "AspNetUsers");
        }
    }
}
