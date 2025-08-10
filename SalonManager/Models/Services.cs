namespace SalonManager.Models
{
    public class Services
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; } // slot length
        public string? Description { get; set; }
    }
}
