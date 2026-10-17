using Microsoft.AspNetCore.Identity;

namespace RaCeDay.Data;

public static class IdentityRoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in new[] { "Admin", "Organiser", "Participant" })
        {
            if (await roleManager.RoleExistsAsync(name))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole(name));
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create role {name}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }
    }
}
