namespace RandomShop.Models.Product
{
    public class ProductListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public bool Selected { get; set; }

        public bool IsFavorite { get; set; }
    }
}
