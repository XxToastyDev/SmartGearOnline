using Microsoft.AspNetCore.Mvc;
using SmartGearOnline.Models;
using SmartGearOnline.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGearOnline.ViewComponents
{
    public class RecentProductsViewComponent : ViewComponent
    {
        private readonly IProductService _productService;

        public RecentProductsViewComponent(IProductService productService)
        {
            _productService = productService;
        }

        // Async method to match IProductService
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Product> products = await _productService.GetRecentProductsAsync();
            return View(products);
        }
    }
}
