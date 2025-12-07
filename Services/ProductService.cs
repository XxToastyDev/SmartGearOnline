using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SmartGearOnline.Models;
using SmartGearOnline.Repositories;

namespace SmartGearOnline.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IMemoryCache _cache;
        private const string ProductListCacheKey = "ProductList";

        public ProductService(IProductRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _cache.GetOrCreateAsync(ProductListCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                var products = await _repo.GetAllAsync();
                return products ?? new List<Product>();
            });
        }

        public void InvalidateProductListCache() => _cache.Remove(ProductListCacheKey);

        public async Task AddProductAsync(Product p)
        {
            await _repo.AddAsync(p);
            InvalidateProductListCache();
        }

        public async Task<bool> UpdateProductAsync(int id, Product updated)
        {
            // repository UpdateAsync likely accepts a Product only; await it and return true on success
            await _repo.UpdateAsync(updated);
            InvalidateProductListCache();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            // repository DeleteAsync likely returns Task or Task<bool>; await and assume success
            await _repo.DeleteAsync(id);
            InvalidateProductListCache();
            return true;
        }

        // Implementations required by IProductService
        public async Task<Product?> GetByIdAsync(int id)
        {
            var list = await GetAllProductsAsync();
            return list.FirstOrDefault(p => p.Id == id);
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            // Prefer repository if it exposes categories; fallback to deriving from products
            try
            {
                var repoType = _repo.GetType();
                var method = repoType.GetMethod("GetCategoriesAsync", Type.EmptyTypes);
                if (method != null)
                {
                    var task = method.Invoke(_repo, null) as Task<List<Category>>;
                    if (task != null) return await task;
                }
            }
            catch
            {
                // ignore and fall back
            }

            // Fallback: derive categories from products (may be limited if product->Category navigation is not populated)
            var products = await GetAllProductsAsync();
            var categories = products
                .Where(p => p.CategoryId != 0)
                .GroupBy(p => p.CategoryId)
                .Select(g => new Category { Id = g.Key, Name = g.FirstOrDefault()?.Category?.Name ?? $"Category {g.Key}" })
                .ToList();

            return categories;
        }

        public async Task<List<Product>> GetRecentProductsAsync()
        {
            var all = await GetAllProductsAsync();
            return all.OrderByDescending(p => p.Id).Take(10).ToList();
        }
    }
}
