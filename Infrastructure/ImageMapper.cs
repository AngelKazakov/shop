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

        public static List<byte[]> ReadImagesAsByteArray(int productId)
        {
            var imageBytesList = new List<byte[]>();
            string productImagePath = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");

            if (!Directory.Exists(productImagePath))
            {
                return imageBytesList;
            }

            try
            {
                var directoryInfo = new DirectoryInfo(productImagePath);

                foreach (var imageFile in directoryInfo.GetFiles())
                {
                    byte[] imageBytes = File.ReadAllBytes(imageFile.FullName);
                    imageBytesList.Add(imageBytes);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error reading images for Product {productId}: {ex.Message}");
                throw new ApplicationException($"Failed to read images for product {productId}", ex);
            }
            return imageBytesList;
        }
    }
}
