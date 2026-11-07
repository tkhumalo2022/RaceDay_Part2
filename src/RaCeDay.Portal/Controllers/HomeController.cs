using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Portal.Models;
using RaCeDay.Portal.Services;

namespace RaCeDay.Portal.Controllers;
public class HomeController(RaceDayApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, string? query, string? window, CancellationToken ct)
    {
        try
        {
            var (session, _) = await api.SessionAsync(Request, ct);
            var (events, status) = await api.GetAsync<List<RaceEvent>>("api/events", Request, ct);
            var matching = EventDiscovery.Find(
                events ?? [], type, query, DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2)), window);
            ViewData["Role"] = session?.Role;
            return View(new EventListModel(matching.Events, session?.Role,
                status == HttpStatusCode.Unauthorized,
                status is HttpStatusCode.OK or HttpStatusCode.Unauthorized ? null : "Events are temporarily unavailable.",
                matching.Filter, matching.Query, matching.Window));
        }
        catch (HttpRequestException)
        {
            return View(new EventListModel([], null, false, "Start the Part 2 API to load events.", null, null));
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return View(new EventListModel([], null, false, "The RaceDay API timed out.", null, null));
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
