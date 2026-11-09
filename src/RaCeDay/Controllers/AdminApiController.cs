using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaCeDay.Data;
using RaCeDay.Models;

namespace RaCeDay.Controllers;

// All administrative data is served by the API, never directly from the MVC portal.
// No destructive cross-organiser event operations or payment settlement controls are exposed.
[ApiController, Route("api/admin"), Authorize(Roles = "Admin"), AutoValidateAntiforgeryToken]
public sealed class AdminApiController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<AdminOverview>> Overview(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(new AdminOverview(
            await db.Users.CountAsync(ct),
            await db.UserRoles.Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .CountAsync(role => role == "Organiser", ct),
            await db.UserRoles.Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .CountAsync(role => role == "Participant", ct),
            await db.Events.CountAsync(ct),
            await db.Categories.CountAsync(ct),
            await db.Enrolments.CountAsync(ct),
            await db.Results.CountAsync(ct),
            await db.Payments.CountAsync(ct),
            await db.Payments.CountAsync(x => x.Status == "Pending", ct),
            await db.Events.CountAsync(x => x.EventDate >= today && x.Status != "Cancelled", ct)));
    }

    [HttpGet("users")]
    public async Task<ActionResult<AdminPage<AdminUserRow>>> Users(
        [FromQuery] string? query = null, [FromQuery] string? role = null,
        [FromQuery] int page = 1, CancellationToken ct = default)
    {
        if (page < 1 || page > 100000) return BadRequest(new { message = "Invalid page." });
        if (query?.Length > 100 || (role is not null && role is not ("Admin" or "Organiser" or "Participant")))
            return BadRequest(new { message = "Invalid user filter." });
        var filtered = db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            filtered = filtered.Where(u => (u.Email != null && u.Email.Contains(term))
                || u.FirstName.Contains(term) || u.LastName.Contains(term));
        }
        if (role is not null)
        {
            var roleIds = db.Roles.Where(r => r.Name == role).Select(r => r.Id);
            var userIds = db.UserRoles.Where(ur => roleIds.Contains(ur.RoleId)).Select(ur => ur.UserId);
            filtered = filtered.Where(u => userIds.Contains(u.Id));
        }
        var count = await filtered.CountAsync(ct);
        const int size = 15;
        var pageUsers = await filtered.OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);
        var ids = pageUsers.Select(x => x.Id).ToArray();
        var roles = await (from ur in db.UserRoles.AsNoTracking()
                           join r in db.Roles.AsNoTracking() on ur.RoleId equals r.Id
                           where ids.Contains(ur.UserId)
                           select new { ur.UserId, r.Name }).ToListAsync(ct);
        var rows = pageUsers.Select(u => new AdminUserRow(u.Id,
            (u.FirstName + " " + u.LastName).Trim(), u.Email ?? "", u.CreatedAt,
            roles.Where(x => x.UserId == u.Id).Select(x => x.Name ?? "").FirstOrDefault() ?? "Unknown",
            u.LockoutEnabled && u.LockoutEnd == DateTimeOffset.MaxValue,
            u.EmailConfirmed)).ToArray();
        return Ok(new AdminPage<AdminUserRow>(count, page, size, rows));
    }

    [HttpGet("events")]
    public async Task<ActionResult<AdminPage<AdminEventRow>>> Events(
        [FromQuery] string? query = null, [FromQuery] int page = 1, CancellationToken ct = default)
    {
        if (page < 1 || page > 100000 || query?.Length > 100)
            return BadRequest(new { message = "Invalid event filter." });
        var source = db.Events.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            source = source.Where(e => e.EventName.Contains(term) || e.Location.Contains(term));
        }
        var total = await source.CountAsync(ct);
        const int size = 12;
        var rows = await (from e in source
            join owner in db.Users.AsNoTracking() on e.OrganiserId equals owner.Id
            orderby e.EventDate descending, e.Id descending
            select new AdminEventRow(e.Id, e.EventName, e.EventDate,
                e.Location, e.Status, (owner.FirstName + " " + owner.LastName).Trim(),
                db.Enrolments.Count(x => x.EventId == e.Id),
                db.Results.Count(x => db.Enrolments.Any(en => en.Id == x.EnrolmentId && en.EventId == e.Id))))
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return Ok(new AdminPage<AdminEventRow>(total, page, size, rows));
    }

    [HttpGet("enrolments")]
    public async Task<ActionResult<AdminPage<AdminEnrolmentRow>>> Enrolments(
        [FromQuery] int page = 1, CancellationToken ct = default)
    {
        if (page < 1 || page > 100000) return BadRequest(new { message = "Invalid page." });
        var total = await db.Enrolments.CountAsync(ct);
        const int size = 12;
        var rows = await (from e in db.Enrolments.AsNoTracking()
            join race in db.Events.AsNoTracking() on e.EventId equals race.Id
            join participant in db.Users.AsNoTracking() on e.ParticipantId equals participant.Id
            orderby e.EnrolmentDate descending, e.Id descending
            select new AdminEnrolmentRow(e.Id, race.EventName,
                (participant.FirstName + " " + participant.LastName).Trim(),
                e.Status, e.EnrolmentDate, e.RaceNumber))
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return Ok(new AdminPage<AdminEnrolmentRow>(total, page, size, rows));
    }

    // Admin-only operational report. CSV values are encoded against spreadsheet
    // formula injection and always quoted; no personal contact data is exported.
    [HttpGet("reports/events.csv")]
    public async Task<IActionResult> ExportEvents(CancellationToken ct)
    {
        var data = await (from e in db.Events.AsNoTracking()
            join owner in db.Users.AsNoTracking() on e.OrganiserId equals owner.Id
            orderby e.EventDate descending, e.Id descending
            select new AdminEventRow(e.Id, e.EventName, e.EventDate,
                e.Location, e.Status, (owner.FirstName + " " + owner.LastName).Trim(),
                db.Enrolments.Count(x => x.EventId == e.Id),
                db.Results.Count(x => db.Enrolments.Any(en => en.Id == x.EnrolmentId && en.EventId == e.Id))))
            .Take(2000).ToListAsync(ct);
        var output = new System.Text.StringBuilder(
            "Event ID,Event,Date,Location,Status,Organiser,Enrolments,Results\r\n");
        foreach (var e in data)
            output.Append(e.Id).Append(',').Append(Csv(e.EventName)).Append(',')
                .Append(e.EventDate.ToString("yyyy-MM-dd")).Append(',')
                .Append(Csv(e.Location)).Append(',').Append(Csv(e.Status)).Append(',')
                .Append(Csv(e.Organiser)).Append(',').Append(e.Enrolments).Append(',')
                .Append(e.Results).Append("\r\n");
        return File(new System.Text.UTF8Encoding(true).GetBytes(output.ToString()),
            "text/csv", "RaceDay-admin-events.csv");
    }

    private static string Csv(string? value)
    {
        var result = value ?? "";
        // Excel may interpret imported cells beginning with a formula prefix.
        if (result.TrimStart().StartsWith('=') || result.TrimStart().StartsWith('+') ||
            result.TrimStart().StartsWith('-') || result.TrimStart().StartsWith('@'))
            result = "'" + result;
        return "\"" + result.Replace("\"", "\"\"") + "\"";
    }

    // Disabling accounts prevents future sign-in and rejects active API sessions
    // via the lockout middleware. Admins cannot disable themselves or other admins.
    [HttpPost("users/{id}/access")]
    public async Task<IActionResult> Access(string id, AdminAccessInput input)
    {
        var target = await users.FindByIdAsync(id);
        if (target is null) return NotFound();
        if (target.Id == users.GetUserId(User) || await users.IsInRoleAsync(target, "Admin"))
            return Conflict(new { message = "Administrator accounts cannot be disabled from here." });
        var change = await users.SetLockoutEnabledAsync(target, true);
        if (!change.Succeeded) return BadRequest(new { message = "Could not update this account." });
        change = await users.SetLockoutEndDateAsync(target,
            input.Disabled ? DateTimeOffset.MaxValue : null);
        if (!change.Succeeded) return BadRequest(new { message = "Could not update this account." });
        change = await users.UpdateSecurityStampAsync(target);
        if (!change.Succeeded) return BadRequest(new { message = "Could not update session security." });
        return Ok(new { disabled = input.Disabled });
    }

    // Role changes are intentionally limited to non-admin roles and accounts with
    // no race records, preserving all existing organiser/participant ownership.
    [HttpPost("users/{id}/role")]
    public async Task<IActionResult> ChangeRole(string id, AdminRoleInput input, CancellationToken ct)
    {
        if (input.Role == "Organiser" && string.IsNullOrWhiteSpace(input.OrganiserName))
            return BadRequest(new { message = "Organisation name required for organiser role." });
        var target = await users.FindByIdAsync(id);
        if (target is null) return NotFound();
        if (target.Id == users.GetUserId(User) || await users.IsInRoleAsync(target, "Admin"))
            return Conflict(new { message = "Administrator roles cannot be changed here." });
        var current = await users.GetRolesAsync(target);
        if (current.Count != 1 || current[0] is not ("Organiser" or "Participant"))
            return Conflict(new { message = "Account role must be reviewed manually." });
        if (current[0] == input.Role) return Ok(new { role = input.Role });
        if (await db.Events.AnyAsync(e => e.OrganiserId == id, ct) ||
            await db.Enrolments.AnyAsync(e => e.ParticipantId == id, ct))
            return Conflict(new { message = "This account has linked race records and cannot change role." });

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var remove = await users.RemoveFromRoleAsync(target, current[0]);
        if (!remove.Succeeded) return BadRequest(new { message = "Could not change account role." });
        var add = await users.AddToRoleAsync(target, input.Role);
        if (!add.Succeeded) return BadRequest(new { message = "Could not assign new role." });
        target.OrganiserName = input.Role == "Organiser" ? input.OrganiserName?.Trim() : null;
        var saved = await users.UpdateAsync(target);
        if (!saved.Succeeded) return BadRequest(new { message = "Could not save account role." });
        var stamp = await users.UpdateSecurityStampAsync(target);
        if (!stamp.Succeeded) return BadRequest(new { message = "Could not refresh account security." });
        await tx.CommitAsync(ct);
        return Ok(new { role = input.Role });
    }
}

public sealed record AdminOverview(
    int Users, int Organisers, int Participants, int Events, int Categories,
    int Enrolments, int Results, int PaymentAttempts, int PendingPaymentAttempts, int UpcomingEvents);
public sealed record AdminPage<T>(int Total, int Page, int PageSize, IReadOnlyList<T> Items);
public sealed record AdminUserRow(
    string Id, string Name, string Email, DateTime CreatedAt,
    string Role, bool Disabled, bool EmailConfirmed);
public sealed record AdminEventRow(
    int Id, string EventName, DateOnly EventDate, string Location,
    string Status, string Organiser, int Enrolments, int Results);
public sealed record AdminEnrolmentRow(
    int Id, string EventName, string Participant, string Status, DateTime EnrolmentDate, string RaceNumber);
public sealed record AdminAccessInput(bool Disabled);
public sealed class AdminRoleInput
{
    [Required, RegularExpression("^(Organiser|Participant)$")]
    public string Role { get; set; } = "";
    [StringLength(100)] public string? OrganiserName { get; set; }
}
