using System.ComponentModel.DataAnnotations;

namespace SalonManager.Models
{
    public class Services
    {
        public int Id { get; set; }      //primary key

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; } // slot length

        [StringLength(500)]
        public string? Description { get; set; }

        public string? ImagePath { get; set; }

        public int? CategoryId { get; set; } // Foreign key
        public ServiceCategory? Category { get; set; } // Navigation property

    }
}
