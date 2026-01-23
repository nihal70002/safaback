using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateECommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRejectedBySalesToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRejectedBySales",
                table: "Orders",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRejectedBySales",
                table: "Orders");
        }
    }
}
