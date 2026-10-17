using Microsoft.AspNetCore.Identity;
using RaCeDay.Models;

namespace RaCeDay.Services;

// A server-side session mirror for the course's explicit user-ID/role session requirement.
// ASP.NET Identity cookie claims, NOT these client-facing session values, remain the
// authoritative source used by [Authorize] and workflow ownership enforcement.
public static class SessionIdentity
{
    private const string UserIdKey = "RaCeDay.UserId";
    private const string RoleKey = "RaCeDay.Role";

    public static async Task RememberAsync(
        HttpContext context,
        UserManager<ApplicationUser> users,
        ApplicationUser user)
    {
        var roles = await users.GetRolesAsync(user);
        var role = roles.Contains("Admin") ? "Admin"
            : roles.Contains("Organiser") ? "Organiser"
            : roles.Contains("Participant") ? "Participant"
            : throw new InvalidOperationException("A signed-in RaCeDay user needs an application role.");

        await context.Session.LoadAsync();
        context.Session.SetString(UserIdKey, user.Id);
        context.Session.SetString(RoleKey, role);
    }

    public static (string? UserId, string? Role) Read(HttpContext context) =>
        (context.Session.GetString(UserIdKey), context.Session.GetString(RoleKey));

    public static void Clear(HttpContext context) => context.Session.Clear();
}
