using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Portal.Models;
using RaCeDay.Portal.Services;

namespace RaCeDay.Portal.Controllers;

// The separate MVC portal never reads or writes database tables directly.
public sealed class AdminController(RaceDayApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? query, string? role, int page = 1, int eventPage = 1, int enrolmentPage = 1, CancellationToken ct = default)
    {
        page = Math.Clamp(page, 1, 100000);
        eventPage = Math.Clamp(eventPage, 1, 100000);
        enrolmentPage = Math.Clamp(enrolmentPage, 1, 100000);
        query = query?.Trim();
        if (query?.Length > 100) query = query[..100];
        if (role is not ("Admin" or "Organiser" or "Participant")) role = null;

        try
        {
            var (session, status) = await api.SessionAsync(Request, ct);
            if (status == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");
            if (session?.Role != "Admin") return StatusCode(403, "Administrator access required.");
            ViewData["Role"] = "Admin";

            var (overview, overviewStatus) =
                await api.GetAsync<AdminOverview>("api/admin/overview", Request, ct);
            var (users, usersStatus) =
                await api.GetAsync<AdminPage<AdminUserRow>>(
                    $"api/admin/users?page={page}&query={Uri.EscapeDataString(query ?? "")}&role={Uri.EscapeDataString(role ?? "")}",
                    Request, ct);
            var (events, eventsStatus) =
                await api.GetAsync<AdminPage<AdminEventRow>>($"api/admin/events?page={eventPage}", Request, ct);
            var (entries, entriesStatus) =
                await api.GetAsync<AdminPage<AdminEnrolmentRow>>($"api/admin/enrolments?page={enrolmentPage}", Request, ct);

            if (overviewStatus == HttpStatusCode.Unauthorized || usersStatus == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");
            if (overviewStatus == HttpStatusCode.Forbidden || usersStatus == HttpStatusCode.Forbidden)
                return StatusCode(403, "Administrator access required.");

            var warning = overviewStatus == HttpStatusCode.OK && usersStatus == HttpStatusCode.OK &&
                eventsStatus == HttpStatusCode.OK && entriesStatus == HttpStatusCode.OK
                ? null : "Some live administrative information is currently unavailable.";
            return View(new AdminDashboardPage(
                overview ?? new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                users ?? new(0, 1, 15, []),
                events ?? new(0, 1, 12, []),
                entries ?? new(0, 1, 12, []),
                query, role, warning));
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "RaceDay API is unavailable.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, "RaceDay API timed out.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> EventsCsv(CancellationToken ct)
    {
        try
        {
            var (data, status) = await api.DownloadReportAsync("api/admin/reports/events.csv", Request, ct);
            if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");
            if (status == HttpStatusCode.Forbidden) return StatusCode(403);
            if (data is null || status != HttpStatusCode.OK)
                return StatusCode(503, "RaceDay event report could not be generated.");
            return File(data, "text/csv", "RaceDay-admin-events.csv");
        }
        catch (HttpRequestException) { return StatusCode(503, "RaceDay API is unavailable."); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        { return StatusCode(504, "RaceDay API timed out."); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAccess(
        string id, bool disabled, string? query, string? role, int page = 1, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        try
        {
            var result = await api.MutateAsync(HttpContext, HttpMethod.Post,
                $"api/admin/users/{Uri.EscapeDataString(id)}/access",
                new { disabled }, ct);
            TempData[result.Successful ? "Notice" : "Error"] = result.Successful
                ? (disabled ? "Account disabled." : "Account re-enabled.")
                : result.Error ?? "Could not update this account. Check your admin permissions.";
        }
        catch (HttpRequestException) { TempData["Error"] = "API is unavailable. No change confirmed."; }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        { TempData["Error"] = "API timed out. Verify the account before retrying."; }
        return RedirectToAction(nameof(Index), new { query, role, page });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(
        string id, string role, string? organiserName, string? query,
        string? roleFilter, int page = 1, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id) || role is not ("Organiser" or "Participant"))
            return BadRequest();
        try
        {
            var result = await api.MutateAsync(HttpContext, HttpMethod.Post,
                $"api/admin/users/{Uri.EscapeDataString(id)}/role",
                new { role, organiserName }, ct);
            TempData[result.Successful ? "Notice" : "Error"] = result.Successful
                ? $"Account role updated to {role}."
                : result.Error ?? "Role change rejected. Check linked race records.";
        }
        catch (HttpRequestException) { TempData["Error"] = "API is unavailable. No change confirmed."; }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        { TempData["Error"] = "API timed out. Verify the role before retrying."; }
        return RedirectToAction(nameof(Index), new { query, role = roleFilter, page });
    }
}
