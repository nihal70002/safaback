using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateECommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class Rejectform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "Orders");
        }
    }
}
