using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Portal.Models;
using RaCeDay.Portal.Services;

namespace RaCeDay.Portal.Controllers;
public class HomeController(RaceDayApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, CancellationToken ct)
    {
        try
        {
            var (session, _) = await api.SessionAsync(Request, ct);
            var (events, status) = await api.GetAsync<List<RaceEvent>>("api/events", Request, ct);
            var filter = new[] { "Run", "Walk", "Cycle" }
                .FirstOrDefault(x => x.Equals(type, StringComparison.OrdinalIgnoreCase));
            var list = (events ?? []).Where(x =>
                x.EventDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2)) &&
                (x.Status == "Open" || x.Status == "Planned")).ToList();
            if (filter is not null) list = list.Where(x => x.EventType == filter).ToList();
            ViewData["Role"] = session?.Role;
            return View(new EventListModel(list, session?.Role,
                status == HttpStatusCode.Unauthorized,
                status is HttpStatusCode.OK or HttpStatusCode.Unauthorized ? null : "Events are temporarily unavailable.",
                filter));
        }
        catch (HttpRequestException)
        {
            return View(new EventListModel([], null, false, "Start the Part 2 API to load events.", null));
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return View(new EventListModel([], null, false, "The RaceDay API timed out.", null));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        if (id <= 0) return NotFound();
        try
        {
            var (race, status) = await api.GetAsync<RaceEvent>($"api/events/{id}", Request, ct);
            if (status == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");
            if (race is null) return NotFound();
            var (categories, _) = await api.GetAsync<List<RaceCategory>>(
                $"api/events/{id}/categories", Request, ct);
            var (session, _) = await api.SessionAsync(Request, ct);
            ViewData["Role"] = session?.Role;
            return View(new EventPageModel(race, categories ?? []));
        }
        catch (HttpRequestException) { return StatusCode(503, "The RaceDay API is unavailable."); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        { return StatusCode(504, "The RaceDay API timed out."); }
    }

    public IActionResult Error() => View();
}
