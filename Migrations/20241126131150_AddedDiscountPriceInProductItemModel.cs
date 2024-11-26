using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandomShop.Migrations
{
    /// <inheritdoc />
    public partial class AddedDiscountPriceInProductItemModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedPrice",
                table: "ProductItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountedPrice",
                table: "ProductItems");
        }
    }
}
