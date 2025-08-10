namespace SalonManager.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; } // quantity available in stock
       
    }
}
