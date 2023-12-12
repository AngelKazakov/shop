namespace RandomShop.Data.Models
{
    public class ShippingMethod
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
    }
}
