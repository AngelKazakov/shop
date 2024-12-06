using RandomShop.Data.Models;
using RandomShop.Models.Image;

namespace RandomShop.Services.Images
{
    public interface IImageService
    {
        Task<List<byte[]>> ReadImagesAsByteArrayAsync(int productId);

        Task<List<ProductImageViewModel>> CreateProductImageViewModelAsync(int productId);

        Task<string> MoveImagesToTempDirectoryAsync(int productId);

        Task PermanentlyDeleteTempImageDirectoryAsync(string tempImagePath);

        Task RestoreImagesFromTempDirectoryAsync(string tempImagePath, int productId);

        public ICollection<ProductImage> CreateProductImages(ICollection<IFormFile> images, int productId);

        public Task SaveImages(ICollection<IFormFile> files, ICollection<ProductImage> productImages);
    }
}
