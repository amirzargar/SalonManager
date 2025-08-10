using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalonManager.Models;
using Stripe;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Services> Services { get; set; }
    public DbSet<Products> Products { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Staff> Staffs { get; set; }
}
