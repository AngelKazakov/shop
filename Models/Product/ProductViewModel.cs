namespace RandomShop.Models.Product
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Brand { get; set; }

        public string Description { get; set; }

        public string SKU { get; set; }

        public string Category { get; set; }

        public string Promotion { get; set; }

        public Dictionary<string, List<string>> VariationsAndOptions = new Dictionary<string, List<string>>();

        public List<byte[]> Images = new List<byte[]>();
    }
}
