using SmartGearOnline.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGearOnline.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(int id, Product updated);
        Task<bool> DeleteProductAsync(int id);
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Product>> GetRecentProductsAsync();
        void InvalidateProductListCache();
    }
}
