using SmartGearOnline.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGearOnline.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id); // <-- change from Product to int
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Product>> GetRecentProductsAsync(int count = 3);
    }
}
