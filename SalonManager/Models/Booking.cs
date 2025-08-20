using System;
using System.ComponentModel.DataAnnotations; // For validation attributes
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonManager.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = "";

        [Required]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        [Display(Name = "Staff")]
        public int? StaffId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartAt { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndAt { get; set; }

        [Required]
        [Display(Name = "Status")]
        public BookingStatus Status { get; set; }

        [StringLength(1000, ErrorMessage = "Notes must be under 1000 characters.")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser? Customer { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Services? Service { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff? Staff { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }
}
