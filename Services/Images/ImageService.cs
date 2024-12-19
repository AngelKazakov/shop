﻿using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Image;

namespace RandomShop.Services.Images
{
    public class ImageService : IImageService
    {
        private readonly ShopContext context;

        public ImageService(ShopContext context)
        {
            this.context = context;
        }

        public async Task<List<byte[]>> ReadImagesAsByteArrayAsync(int productId)
        {
            string productImagePath = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");
            var imageBytesList = new List<byte[]>();

            if (!Directory.Exists(productImagePath))
                return imageBytesList;

            var directoryInfo = new DirectoryInfo(productImagePath);

            foreach (var imageFile in directoryInfo.GetFiles())
            {
                imageBytesList.Add(await File.ReadAllBytesAsync(imageFile.FullName));
            }

            return imageBytesList;
        }

        public async Task SaveImages(ICollection<IFormFile> files, ICollection<ProductImage> productImages)
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

        private async Task<List<ProductImage>> GetProductImagesByProductId(int productId)
        {
            List<ProductImage>? productImages = await context.ProductImages
            .AsNoTracking()
            .Where(pi => pi.ProductId == productId)
            .ToListAsync();

            return productImages;
        }

        public async Task<List<ProductImageViewModel>> CreateProductImageViewModelAsync(int productId)
        {
            List<ProductImage>? productImages = await GetProductImagesByProductId(productId);

            List<byte[]>? imageBytesListFromFileSystem = await ReadImagesAsByteArrayAsync(productId);

            // Ensure one-to-one mapping between product images and byte arrays
            if (productImages.Count != imageBytesListFromFileSystem.Count)
            {
                throw new InvalidOperationException("Mismatch between product images and image bytes in the file system.");
            }

            List<ProductImageViewModel>? productImageViewModels = new List<ProductImageViewModel>();

            for (int i = 0; i < productImages.Count; i++)
            {
                productImageViewModels.Add(new ProductImageViewModel()
                {
                    ProductImageId = productImages[i].Id,
                    bytes = imageBytesListFromFileSystem[i],
                });
            }

            return productImageViewModels;
        }


        public ICollection<ProductImage> CreateProductImages(ICollection<IFormFile> images, int productId)
        {
            List<ProductImage>? productImages = new List<ProductImage>();
            string productDirectory = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");

            Directory.CreateDirectory(productDirectory);

            foreach (var image in images)
            {
                if (image != null)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName).ToLower()}";
                    string fullPath = Path.Combine(productDirectory, uniqueFileName);

                    ProductImage? productImage = new ProductImage
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

        public Task<string> MoveImagesToTempDirectoryAsync(int productId)
        {
            string productImagesPath = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");
            string tempFolder = Path.Combine(DataConstants.ImagesPath, "Temp");
            string tempPath = Path.Combine(tempFolder, $"Product{productId}_{Guid.NewGuid()}");

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            if (Directory.Exists(productImagesPath))
            {
                Directory.Move(productImagesPath, tempPath);
            }

            return Task.FromResult(tempPath);
        }
        public Task PermanentlyDeleteTempImageDirectoryAsync(string tempImagePath)
        {
            if (Directory.Exists(tempImagePath))
            {
                Directory.Delete(tempImagePath, recursive: true);
            }

            return Task.CompletedTask;
        }

        public Task RestoreImagesFromTempDirectoryAsync(string tempImagePath, int productId)
        {
            string originalPath = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");

            if (Directory.Exists(tempImagePath))
            {
                Directory.Move(tempImagePath, originalPath);
            }

            return Task.CompletedTask;
        }
    }
}