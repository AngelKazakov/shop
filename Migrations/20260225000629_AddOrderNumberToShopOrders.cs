using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandomShop.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumberToShopOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add column as nullable first (so it can be added even with existing rows)
            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "ShopOrders",
                type: "nvarchar(32)",
                nullable: true);

            // 2) Backfill existing rows with a unique value based on Id
            //    Example: RS-00000001
            migrationBuilder.Sql(@"
            UPDATE so
            SET so.OrderNumber = 'RS-' + RIGHT('00000000' + CAST(so.Id AS varchar(8)), 8)
            FROM ShopOrders so
            WHERE so.OrderNumber IS NULL OR so.OrderNumber = '';
        ");

            // 3) Make it NOT NULL after backfill
            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "ShopOrders",
                type: "nvarchar(32)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldNullable: true);

            // 4) Create UNIQUE index
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

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "ShopOrders");
        }
    }
}
