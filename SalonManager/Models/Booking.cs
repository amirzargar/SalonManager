namespace SalonManager.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = "";
        public int ServiceId { get; set; }
        public int? StaffId { get; set; }         
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public BookingStatus Status { get; set; }
        public string? Notes { get; set; }

        public virtual ApplicationUser? Customer { get; set; }
        public Services Service { get; set; } = null!;
        public Staff? Staff { get; set; }  

    }
    public enum BookingStatus { Pending, Confirmed, Cancelled, Completed }
}
