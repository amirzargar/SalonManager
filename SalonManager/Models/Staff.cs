namespace SalonManager.Models
{
    public class Staff
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;

        public string? ApplicationUserId { get; set; }                  // Foreign key to ApplicationUser
        public virtual ApplicationUser? ApplicationUser { get; set; }     // Navigation property for ApplicationUser

       


    }
}
