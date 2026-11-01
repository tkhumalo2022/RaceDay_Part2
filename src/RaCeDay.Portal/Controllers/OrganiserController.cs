using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Portal.Models;
using RaCeDay.Portal.Services;

namespace RaCeDay.Portal.Controllers;

// All state changes go to the Part 2 API. Authorisation is enforced by that API,
// not merely by this controller's navigation and session checks.
public sealed class OrganiserController(RaceDayApiClient api) : Controller
{
    private async Task<bool> IsOrganiser(CancellationToken ct)
    {
        var (session, _) = await api.SessionAsync(Request, ct);
        ViewData["Role"] = session?.Role;
        return session?.Role == "Organiser";
    }
    private IActionResult SignIn() => ViewData["Role"] is string
        ? StatusCode(403, "Organiser access required.") : RedirectToAction("Login", "Account");

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        return View("Edit", new EventForm());
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        var (race, status) = await api.GetAsync<RaceEvent>($"api/events/{id}", Request, ct);
        if (status == HttpStatusCode.NotFound || race is null) return NotFound();
        if (!race.IsOwner) return StatusCode(403, "Only the event owner can manage this event.");
        return View(EventForm.From(race));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(EventForm model, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        if (!ModelState.IsValid) return View("Edit", model);
        var creating = model.Id == 0;
        var result = await api.MutateAsync(HttpContext,
            creating ? HttpMethod.Post : HttpMethod.Put,
            creating ? "api/events" : $"api/events/{model.Id}", model, ct);
        if (!result.Successful)
        {
            ModelState.AddModelError("", result.Error ?? $"Could not save event ({(int)result.Status}).");
            return View("Edit", model);
        }
        TempData["Notice"] = "Event saved.";
        return RedirectToAction("Details", "Home", new { id = result.Id ?? model.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        var result = await api.MutateAsync<object>(HttpContext, HttpMethod.Delete,
            $"api/events/{id}", null, ct);
        TempData["Notice"] = result.Successful ? "Event deleted." :
            result.Error ?? "The event could not be deleted (existing entries may prevent deletion).";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> Categories(int id, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        var (race, _) = await api.GetAsync<RaceEvent>($"api/events/{id}", Request, ct);
        if (race is null) return NotFound();
        if (!race.IsOwner) return StatusCode(403, "Only the event owner can manage this event.");
        var (categories, _) = await api.GetAsync<List<RaceCategory>>(
            $"api/events/{id}/categories", Request, ct);
        return View(new CategoriesPage(race, categories ?? []));
    }

    [HttpGet]
    public async Task<IActionResult> Category(int eventId, int id, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        var (race, _) = await api.GetAsync<RaceEvent>($"api/events/{eventId}", Request, ct);
        if (race is null) return NotFound();
        if (!race.IsOwner) return StatusCode(403, "Only the event owner can manage this event.");
        var form = new CategoryForm { EventId = eventId };
        if (id > 0)
        {
            var (list, _) = await api.GetAsync<List<RaceCategory>>(
                $"api/events/{eventId}/categories", Request, ct);
            var item = list?.FirstOrDefault(x => x.Id == id);
            if (item is null) return NotFound();
            form.Id=item.Id;form.CategoryName=item.CategoryName;form.DistanceKm=item.DistanceKm;
            form.EntryFee=item.EntryFee;form.MaxParticipants=item.MaxParticipants;
        }
        return View(form);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CategoryForm model, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        if (!ModelState.IsValid) return View("Category", model);
        var creating = model.Id == 0;
        var result = await api.MutateAsync(HttpContext,
            creating ? HttpMethod.Post : HttpMethod.Put,
            creating ? $"api/events/{model.EventId}/categories" : $"api/categories/{model.Id}", model, ct);
        if (!result.Successful)
        {
            ModelState.AddModelError("", result.Error ?? "Could not save category.");
            return View("Category", model);
        }
        TempData["Notice"] = "Category saved.";
        return RedirectToAction("Categories", new { id = model.EventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        var result = await api.MutateAsync<object>(
            HttpContext, HttpMethod.Delete, $"api/categories/{id}", null, ct);
        TempData["Notice"] = result.Successful ? "Category deleted." :
            result.Error ?? "Category could not be deleted.";
        return RedirectToAction("Categories", new { id = eventId });
    }

    [HttpGet]
    public async Task<IActionResult> Entries(int id, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        var (race, _) = await api.GetAsync<RaceEvent>($"api/events/{id}", Request, ct);
        if (race is null) return NotFound();
        if (!race.IsOwner) return StatusCode(403, "Only the event owner can manage this event.");
        var (entries, status) = await api.GetAsync<List<RaceEntry>>(
            $"api/events/{id}/enrolments", Request, ct);
        if (status != HttpStatusCode.OK)
            TempData["Notice"] = "Enrolments could not be loaded.";
        return View(new OrganiserEntriesPage(race, entries ?? []));
    }

    [HttpGet]
    public async Task<IActionResult> Result(int eventId, int enrolmentId, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        var (race, _) = await api.GetAsync<RaceEvent>($"api/events/{eventId}", Request, ct);
        if (race is null) return NotFound();
        if (!race.IsOwner) return StatusCode(403, "Only the event owner can manage this event.");
        var (entries, _) = await api.GetAsync<List<RaceEntry>>(
            $"api/events/{eventId}/enrolments", Request, ct);
        var entry = entries?.FirstOrDefault(x => x.Id == enrolmentId);
        if (entry is null) return NotFound();
        return View(new ResultForm {
            EventId=eventId, EnrolmentId=enrolmentId,
            EventName=race.EventName, ParticipantName=entry.ParticipantName
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(ResultForm model, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        if (model.ResultStatus == "Finished" &&
            (model.StartTime is null || model.FinishTime is null || model.OverallPosition is null))
            ModelState.AddModelError("", "A finisher needs start/finish times and overall position.");
        if (!ModelState.IsValid) return View("Result", model);
        var result = await api.MutateAsync(HttpContext, HttpMethod.Post,
            $"api/enrolments/{model.EnrolmentId}/results", model, ct);
        if (!result.Successful)
        {
            ModelState.AddModelError("", result.Error ?? "Could not record race result.");
            return View("Result", model);
        }
        TempData["Notice"] = "Race result recorded.";
        return RedirectToAction("Entries", new { id=model.EventId });
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(4_194_304)]
    public async Task<IActionResult> Banner(int id, IFormFile? file, CancellationToken ct)
    {
        if (!await IsOrganiser(ct)) return SignIn();
        if (file is null || file.Length is < 1 or > 3 * 1024 * 1024)
        {
            TempData["Notice"] = "Choose a JPG or PNG under 3 MB.";
            return RedirectToAction("Details", "Home", new { id });
        }
        var result = await api.MutateAsync<object>(HttpContext, HttpMethod.Post,
            $"api/events/{id}/banner", null, ct, image:file);
        TempData["Notice"] = result.Successful ? "Banner saved to private image storage." :
            result.Error ?? "Could not upload banner. Check Azure configuration.";
        return RedirectToAction("Details", "Home", new { id });
    }
}