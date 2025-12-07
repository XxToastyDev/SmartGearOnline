using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartGearOnline.Models;

namespace SmartGearOnline.Data
{
    public class SmartGearContext : IdentityDbContext<ApplicationUser>
    {
        public SmartGearContext(DbContextOptions<SmartGearContext> options)
            : base(options)
        {
        }

        // add Categories DbSet so repositories can access context.Categories
        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // ensure Identity model is added

            // prevent decimal truncation warning — adjust precision/scale as needed
            builder.Entity<Product>()
                   .Property(p => p.BasePrice)
                   .HasPrecision(18, 2);

            // Seed categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Balls" },
                new Category { Id = 2, Name = "Footwear" },
                new Category { Id = 3, Name = "Clothing" }
            );

            // Seed products
            builder.Entity<Product>().HasData(
                new Product 
                { 
                    Id = 1, 
                    Name = "Soccer Ball", 
                    Description = "Size 5 training ball", 
                    BasePrice = 250, 
                    MarkupPercentage = 20, 
                    CategoryId = 1 
                },
                new Product 
                { 
                    Id = 2, 
                    Name = "Rugby Boots", 
                    Description = "Studded boots", 
                    BasePrice = 700, 
                    MarkupPercentage = 15, 
                    CategoryId = 2 
                },
                new Product 
                { 
                    Id = 3, 
                    Name = "Custom Jersey", 
                    Description = "Team jersey", 
                    BasePrice = 300, 
                    MarkupPercentage = 30, 
                    CategoryId = 3 
                }
            );
        }
    }
}
