using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateECommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesExecutiveRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_SalesExecutiveId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_CompanyName",
                table: "Users",
                column: "CompanyName");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_SalesExecutiveId",
                table: "Users",
                column: "SalesExecutiveId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_SalesExecutiveId",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_CompanyName",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Email",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_SalesExecutiveId",
                table: "Users",
                column: "SalesExecutiveId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
