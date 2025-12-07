using Microsoft.EntityFrameworkCore;
using SmartGearOnline.Data;
using SmartGearOnline.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartGearOnline.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly SmartGearContext _context;

        public ProductRepository(SmartGearContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                                 .OrderBy(p => p.Id)
                                 .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        // Updated to take an id
        public async Task DeleteAsync(int id)
{
    var product = await _context.Products.FindAsync(id);
    if (product != null)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}


        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                                 .OrderBy(c => c.Id)
                                 .ToListAsync();
        }

        public async Task<List<Product>> GetRecentProductsAsync(int count = 3)
        {
            return await _context.Products
                                 .OrderByDescending(p => p.Id)
                                 .Take(count)
                                 .ToListAsync();
        }
    }
}
