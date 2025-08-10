using System.ComponentModel.DataAnnotations;     
namespace SalonManager.Models
{
    public class ServiceCategory
    {
        public int Id { get; set; } // Primary key

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(250)]
        public string? Description { get; set; }


        // Navigation property for related services
        public ICollection<Services>? Services { get; set; }
    }
}
