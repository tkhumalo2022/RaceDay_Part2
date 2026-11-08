using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaCeDay.Data;
using RaCeDay.Models;
using RaCeDay.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Production requires confirmed email and a real sender. The opt-in staging flag
    // permits disposable peer-review accounts, never production accounts.
    var allowReviewAccounts = builder.Environment.IsStaging() &&
        builder.Configuration.GetValue<bool>("RaceDay:AllowUnconfirmedReviewAccounts");
    options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment() && !allowReviewAccounts;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(45);
    options.Cookie.Name = ".RaCeDay.ServerSession";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddScoped<RaceWorkflow>();
builder.Services.AddScoped<ProfileImageStorage>();
builder.Services.AddScoped<AzureRaceImageStore>();
builder.Services.AddControllersWithViews(options => options.Filters.Add<WorkflowExceptionFilter>());
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api")) context.Response.StatusCode = 401;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api")) context.Response.StatusCode = 403;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RaCeDay API", Version = "v1",
        Description = "This API uses ASP.NET Identity cookie sessions. For public register/login calls, first GET /api/auth/antiforgery and enter its requestToken in Swagger Authorize as X-CSRF-TOKEN. Refresh the token after signing in for authenticated writes. No JWT is issued." });
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    options.AddSecurityDefinition("CSRF", new OpenApiSecurityScheme { Type = SecuritySchemeType.ApiKey, In = ParameterLocation.Header, Name = "X-CSRF-TOKEN" });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "CSRF" } }] = Array.Empty<string>()
    });
});

var app = builder.Build();

// Explicit, Development-only database initialisation for the isolated Docker Compose demo.
// Production still requires reviewed EF migrations and must never self-initialise.
if (args.Contains("--init-demo-db"))
{
    if (!app.Environment.IsDevelopment())
        throw new InvalidOperationException("Demo database initialisation is Development-only.");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider);
    Console.WriteLine("RaceDay local Docker demo database migrated; role names seeded.");
    return;
}

// Schema changes are applied explicitly with EF tooling after review/backup.
// This command seeds role names only; it never grants a user a privileged role.
if (args.Contains("--seed-roles"))
{
    using var scope = app.Services.CreateScope();
    await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider);
    return;
}

// Optional, local-only account setup for demonstrations. No passwords are stored in the source.
if (args.Contains("--create-admin"))
{
    if (!app.Environment.IsDevelopment())
        throw new InvalidOperationException("Admin account setup is available in Development only.");

    var email = Environment.GetEnvironmentVariable("RACEDAY_ADMIN_EMAIL")?.Trim();
    var password = Environment.GetEnvironmentVariable("RACEDAY_ADMIN_PASSWORD");
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        throw new InvalidOperationException("Set RACEDAY_ADMIN_EMAIL and RACEDAY_ADMIN_PASSWORD before using --create-admin.");

    using var scope = app.Services.CreateScope();
    await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider);
    var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (await users.FindByEmailAsync(email) is not null)
        throw new InvalidOperationException("That account already exists. No changes were made.");

    var admin = new ApplicationUser
    {
        UserName = email,
        Email = email,
        FirstName = "RaceDay",
        LastName = "Administrator",
        EmailConfirmed = true
    };
    var created = await users.CreateAsync(admin, password);
    if (!created.Succeeded)
        throw new InvalidOperationException(
            "Could not create the admin account: " + string.Join("; ", created.Errors.Select(e => e.Description)));
    var assigned = await users.AddToRoleAsync(admin, "Admin");
    if (!assigned.Succeeded)
    {
        await users.DeleteAsync(admin);
        throw new InvalidOperationException("Could not assign the Admin role.");
    }
    Console.WriteLine("Admin account created. Sign in using the email and password you provided.");
    return;
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseSession();
app.UseAuthentication();
// A disabled review account must lose API access immediately, not just after the
// normal Identity security-stamp validation interval. Applies to authenticated
// API and legacy MVC routes; the portal itself only relays API sessions.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var id = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(id))
        {
            var users = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var account = await users.FindByIdAsync(id);
            // A role change also invalidates old cookies immediately. A removed
            // Organiser must not retain access through stale role claims.
            var oldRoles = context.User.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(claim => claim.Value).ToHashSet(StringComparer.Ordinal);
            var effectiveRoles = account is null
                ? new HashSet<string>(StringComparer.Ordinal)
                : (await users.GetRolesAsync(account)).ToHashSet(StringComparer.Ordinal);
            if (account is null ||
                (account.LockoutEnabled && account.LockoutEnd == DateTimeOffset.MaxValue) ||
                !oldRoles.SetEquals(effectiveRoles))
            {
                await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                if (context.Request.Path.StartsWithSegments("/api"))
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                else
                    context.Response.Redirect("/Identity/Account/Login");
                return;
            }
        }
    }
    await next();
});
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
// Health endpoints disclose no connection strings, SQL details, or user data.
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet("/readyz", async (ApplicationDbContext db, CancellationToken ct) =>
{
    try
    {
        // Check that the SQL Server schema exists, not merely that TCP is reachable.
        _ = await db.Users.AsNoTracking().AnyAsync(ct);
        return Results.Ok(new { status = "ready" });
    }
    catch (Exception)
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});
app.Run();
