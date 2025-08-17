using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SalonManager.Models { 
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? FullName { get; set; }
    public virtual ICollection<Booking>? Bookings { get; set; }   // Navigation property for bookings
    public virtual Staff? StaffProfile { get; set; }           // Navigation property for staff profile

}
}