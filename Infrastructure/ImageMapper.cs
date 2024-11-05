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

        public static async Task SaveImages(ICollection<IFormFile> files, ICollection<ProductImage> productImages)
        {
            if (files == null || files.Count == 0) return;

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var productImage = productImages.FirstOrDefault(pi => pi.Name == formFile.FileName);
                    if (productImage != null)
                    {
                        await using var stream = new FileStream(productImage.FullPath, FileMode.Create, FileAccess.Write);
                        await formFile.CopyToAsync(stream);
                    }
                }
            }
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

        public static string MoveImagesToTempDirectory(int productId)
        {
            string productImagesPath = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");
            string tempFolder = Path.Combine(DataConstants.ImagesPath, "Temp");
            string tempPath = Path.Combine(tempFolder, $"Product{productId}_{Guid.NewGuid()}");

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            Console.WriteLine($"Product images path: {productImagesPath}");
            Console.WriteLine($"Temporary path: {tempPath}");

            if (Directory.Exists(productImagesPath))
            {
                Directory.Move(productImagesPath, tempPath);
            }
            else
            {
                Console.WriteLine("Directory does not exist: " + productImagesPath);
            }

            return tempPath;
        }

        public static void PermanentlyDeleteTempImageDirectory(string tempImagePath)
        {
            if (Directory.Exists(tempImagePath))
            {
                Directory.Delete(tempImagePath, recursive: true);
            }
        }

        public static void RestoreImagesFromTempDirectory(string tempImagePath, int productId)
        {
            string originalPath = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");

            if (Directory.Exists(tempImagePath))
            {
                Directory.Move(tempImagePath, originalPath);
            }
        }
    }
}
