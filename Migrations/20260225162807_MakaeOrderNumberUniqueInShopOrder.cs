using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandomShop.Migrations
{
    /// <inheritdoc />
    public partial class MakaeOrderNumberUniqueInShopOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopOrders_OrderNumber",
                table: "ShopOrders");

            migrationBuilder.CreateIndex(
                name: "IX_ShopOrders_OrderNumber",
                table: "ShopOrders",
                column: "OrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopOrders_OrderNumber",
                table: "ShopOrders");

            migrationBuilder.CreateIndex(
                name: "IX_ShopOrders_OrderNumber",
                table: "ShopOrders",
                column: "OrderNumber");
        }
    }
}
