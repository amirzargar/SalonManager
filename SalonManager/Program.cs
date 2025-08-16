using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using SalonManager.Models;
using SalonManager.EmailServices;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (only register ONCE to avoid duplicate scheme error)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>() // if you plan to use roles
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Email sender service
builder.Services.AddSingleton<IEmailSender, DummyEmailSender>();

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(                                          // Map default controller route
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();                                           // Map Razor Pages



// --- Role and Admin User Seeder ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Admin", "Staff", "Customer" };
    foreach (var role in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Admin user details
    var adminEmail = "admin@salon.com";
    var adminPassword = "Admin@123";  // For production, use something more secure

    // See if admin user exists
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            DisplayName = "Admin"
        };
        var result = await userManager.CreateAsync(newAdmin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
        // (Optional) handle errors/log if result not succeeded.
    }
}

app.Run();
