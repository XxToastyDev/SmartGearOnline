using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartGearOnline.Hubs;
using SmartGearOnline.Models;
using SmartGearOnline.Filters;
using SmartGearOnline.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SmartGearOnline.Controllers
{
    [Authorize]                // require authenticated users by default
    [Route("products")]
    [ServiceFilter(typeof(LoggingFilter))]
    [ServiceFilter(typeof(SimpleAuthorizationFilter))]
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        private readonly IHubContext<ProductHub> _hub;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService service, IHubContext<ProductHub> hub, ILogger<ProductController> logger)
        {
            _service = service;
            _hub = hub;
            _logger = logger;
        }

        [AllowAnonymous]        // public list
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _service.GetAllProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading product list.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // partial that returns only the product list HTML (used by client JS)
        [AllowAnonymous]
        [HttpGet("listpartial")]
        public async Task<IActionResult> ListPartial()
        {
            var products = await _service.GetAllProductsAsync();
            return PartialView("_ProductList", products);
        }

        [AllowAnonymous]        // public details
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _service.GetByIdAsync(id);
                if (product == null) return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading product details for ID {id}.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Categories = await _service.GetCategoriesAsync();
                // ensure required members are set (avoid CS9035 when Product has required members)
                return View(new Product
                {
                    Name = string.Empty,
                    BasePrice = 0,
                    MarkupPercentage = 0,
                    CategoryId = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create product form.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, string user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _service.GetCategoriesAsync();
                    return View(product);
                }

                await _service.AddProductAsync(product);
                // notify clients
                await _hub.Clients.All.SendAsync("ProductChanged", "added", product.Id);
                _logger.LogInformation("Product created {Id}", product.Id);

                return RedirectToAction("Index", new { user = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _service.GetByIdAsync(id);
                if (product == null) return NotFound();

                ViewBag.Categories = await _service.GetCategoriesAsync();
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit page.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product updated, string user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _service.GetCategoriesAsync();
                    return View(updated);
                }

                var ok = await _service.UpdateProductAsync(id, updated);
                if (ok)
                {
                    await _hub.Clients.All.SendAsync("ProductChanged", "updated", id);
                    _logger.LogInformation("Product updated {Id}", id);
                    return RedirectToAction("Index", new { user = user });
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {id}.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _service.GetByIdAsync(id);
                if (product == null) return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading delete confirmation for {id}.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string user)
        {
            try
            {
                var ok = await _service.DeleteProductAsync(id);
                if (ok)
                {
                    await _hub.Clients.All.SendAsync("ProductChanged", "deleted", id);
                    _logger.LogInformation("Product deleted {Id}", id);
                    return RedirectToAction("Index", new { user = user });
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {id}.");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
