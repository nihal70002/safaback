using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateECommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesExecutiveToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesExecutiveId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SalesExecutiveId",
                table: "Users",
                column: "SalesExecutiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_SalesExecutiveId",
                table: "Users",
                column: "SalesExecutiveId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_SalesExecutiveId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SalesExecutiveId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SalesExecutiveId",
                table: "Users");
        }
    }
}
