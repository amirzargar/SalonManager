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
    public DbSet<ServiceCategory> ServiceCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Staff>()
            .HasOne(s => s.ApplicationUser)
            .WithOne(u => u.StaffProfile)
            .HasForeignKey<Staff>(s => s.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }



}
