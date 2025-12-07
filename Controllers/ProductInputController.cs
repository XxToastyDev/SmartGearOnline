using Microsoft.AspNetCore.Mvc;
using SmartGearOnline.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartGearOnline.Controllers
{
    public class ProductInputController : Controller
    {
        private static readonly List<Category> _categories = new()
        {
            new Category { Id = 1, Name = "Sport Equipment" },
            new Category { Id = 2, Name = "Footwear" },
            new Category { Id = 3, Name = "Accessories" }
        };

        private static readonly List<Product> _products = new();

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _categories;
            return View(new Product { Name = "" }); // Initialize required Name
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            ViewBag.Categories = _categories;

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            product.Id = _products.Count + 1;
            product.Category = _categories.First(c => c.Id == product.CategoryId);

            _products.Add(product);

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
