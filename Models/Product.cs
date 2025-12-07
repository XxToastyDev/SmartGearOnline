using System.ComponentModel.DataAnnotations;

namespace SmartGearOnline.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public required string Name { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        // Base price entered by user
        [Range(1, 100000)]
        public decimal BasePrice { get; set; }

        // Business logic markup percentage
        [Range(0, 100)]
        public int MarkupPercentage { get; set; }

        // Computed value - NOT saved, just calculated
        public decimal FinalPrice => BasePrice + (BasePrice * MarkupPercentage / 100m);

        // Foreign key
        [Required]
        public int CategoryId { get; set; }

        // Navigation property (needed for views & controllers)
        public Category? Category { get; set; }
    }
}
