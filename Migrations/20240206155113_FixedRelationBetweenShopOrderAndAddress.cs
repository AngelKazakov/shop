using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandomShop.Migrations
{
    public partial class FixedRelationBetweenShopOrderAndAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopOrders_Addresses_AddressId",
                table: "ShopOrders");

            migrationBuilder.RenameColumn(
                name: "AddressId",
                table: "ShopOrders",
                newName: "ShippingAddressId");

            migrationBuilder.RenameIndex(
                name: "IX_ShopOrders_AddressId",
                table: "ShopOrders",
                newName: "IX_ShopOrders_ShippingAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopOrders_Addresses_ShippingAddressId",
                table: "ShopOrders",
                column: "ShippingAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopOrders_Addresses_ShippingAddressId",
                table: "ShopOrders");

            migrationBuilder.RenameColumn(
                name: "ShippingAddressId",
                table: "ShopOrders",
                newName: "AddressId");

            migrationBuilder.RenameIndex(
                name: "IX_ShopOrders_ShippingAddressId",
                table: "ShopOrders",
                newName: "IX_ShopOrders_AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopOrders_Addresses_AddressId",
                table: "ShopOrders",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
