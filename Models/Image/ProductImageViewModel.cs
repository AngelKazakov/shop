namespace RandomShop.Models.Image
{
    public class ProductImageViewModel
    {
        public int Id { get; set; }

        public int ProductImageId { get; set; }

        public byte[] bytes { get; set; } = new byte[0];
    }
}
