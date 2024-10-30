using RandomShop.Data;
using RandomShop.Data.Models;

namespace RandomShop.Infrastructure
{
    public static class ImageMapper
    {
        public static ICollection<ProductImage> CreateProductImages(ICollection<IFormFile> images, int productId)
        {
            var productImages = new List<ProductImage>();
            string productDirectory = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");

            Directory.CreateDirectory(productDirectory);

            foreach (var image in images)
            {
                if (image != null)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName).ToLower()}";
                    string fullPath = Path.Combine(productDirectory, uniqueFileName);

                    var productImage = new ProductImage
                    {
                        Name = image.FileName,
                        UniqueName = uniqueFileName,
                        ProductId = productId,
                        FullPath = fullPath,
                    };

                    productImages.Add(productImage);
                }
            }
            return productImages;
        }
    }
}
