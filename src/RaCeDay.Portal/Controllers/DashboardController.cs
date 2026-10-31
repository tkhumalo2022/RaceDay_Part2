using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Portal.Models;
using RaCeDay.Portal.Services;

namespace RaCeDay.Portal.Controllers;
public sealed class DashboardController(RaceDayApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var (session, status) = await api.SessionAsync(Request, ct);
            if (status == HttpStatusCode.Unauthorized || session is null)
                return RedirectToAction("Login", "Account");
            ViewData["Role"] = session.Role;
            if (session.Role == "Organiser")
            {
                var (owned, result) = await api.GetAsync<List<RaceEvent>>(
                    "api/organiser/events", Request, ct);
                return View(new DashboardPage("Organiser", owned ?? [], [], [],
                    result == HttpStatusCode.OK ? null : "Could not load your events."));
            }
            if (session.Role == "Participant")
            {
                var (entries, result) = await api.GetAsync<List<RaceEntry>>(
                    "api/enrolments/me", Request, ct);
                var (history, historyStatus) = await api.GetAsync<List<RaceEntry>>(
                    "api/results/me", Request, ct);
                return View(new DashboardPage("Participant", [], entries ?? [], history ?? [],
                    result == HttpStatusCode.OK && historyStatus == HttpStatusCode.OK
                        ? null : "Some account information is unavailable."));
            }
            return Forbid();
        }
        catch (HttpRequestException) { return StatusCode(503, "RaceDay API is unavailable."); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        { return StatusCode(504, "RaceDay API timed out."); }
    }
}
