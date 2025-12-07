using System.ComponentModel.DataAnnotations;

namespace SmartGearOnline.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public required string Name { get; set; }
    }
}
