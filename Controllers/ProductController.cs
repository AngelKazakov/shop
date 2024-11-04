﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Models.Product;
using RandomShop.Models.Variation;
using RandomShop.Services.Products;
using RandomShop.Services.Variation;

namespace RandomShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService productService;
        private readonly IVariationService variationService;
        public ProductController(IProductService productService, IVariationService variationService)
        {
            this.productService = productService;
            this.variationService = variationService;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int categoryId)
        {
            return View(await this.productService.InitProductAddFormModel(categoryId));
        }

        [HttpPost]
        public async Task<IActionResult> Add(ProductAddFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int addedProductId = await this.productService.AddProduct(model);

            return Redirect($"Product/Details/{addedProductId}");
        }

        [HttpGet]
        public async Task<JsonResult> GetVariationOptionsByCategory(int categoryId)
        {
            List<VariationOptionViewModel> variations = await this.variationService.GetVariationOptionBySpecifyCategory(categoryId);
            return Json(variations);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ProductViewModel productDetails = await this.productService.GetProductById(id);

            if (productDetails is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(productDetails);
        }

        [HttpPost]
        public async Task<IActionResult> SearchProducts(string productName)
        {
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            ICollection<ProductListViewModel> productList = await this.productService.GetAllProducts();

            return View(productList);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            bool isProductDeleted = await this.productService.DeleteProduct(id);

            if (isProductDeleted)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
