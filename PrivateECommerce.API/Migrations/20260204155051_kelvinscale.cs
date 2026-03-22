using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateECommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class kelvinscale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "ProductVariants",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductCode",
                table: "ProductVariants",
                column: "ProductCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductCode",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "ProductVariants");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode",
                unique: true);
        }
    }
}
