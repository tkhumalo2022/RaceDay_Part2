using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaCeDay.Data;
using RaCeDay.Models;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Development-only bypass: production requires verified email and a real sender.
    options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment();
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Schema changes are applied explicitly with EF tooling after review/backup.
// This command seeds role names only; it never grants a user a privileged role.
if (args.Contains("--seed-roles"))
{
    using var scope = app.Services.CreateScope();
    await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider);
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
