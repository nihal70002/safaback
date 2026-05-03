using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateECommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityIndexesAndStockMovementOrderRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users",
                columns: new[] { "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CreatedAt",
                table: "StockMovements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_OrderId",
                table: "StockMovements",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId_IsActive",
                table: "Products",
                columns: new[] { "BrandId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsActive",
                table: "Products",
                columns: new[] { "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Expiry_IsUsed",
                table: "PasswordResetTokens",
                columns: new[] { "Expiry", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Orders_OrderId",
                table: "StockMovements",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Orders_OrderId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_CreatedAt",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_OrderId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Expiry_IsUsed",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }
    }
}
