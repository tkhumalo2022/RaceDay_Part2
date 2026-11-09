using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Portal.Models;
using RaCeDay.Portal.Services;

namespace RaCeDay.Portal.Controllers;
public sealed class ParticipantController(RaceDayApiClient api) : Controller
{
    private async Task<ApiSession?> Session(CancellationToken ct)
    {
        var (session, _) = await api.SessionAsync(Request, ct);
        ViewData["Role"] = session?.Role;
        return session;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId, CancellationToken ct)
    {
        var session = await Session(ct);
        if (session is null) return RedirectToAction("Login", "Account");
        if (session.Role != "Participant") return StatusCode(403, "Participant access required.");
        if (eventId <= 0 || categoryId <= 0) return BadRequest();
        var result = await api.MutateAsync(HttpContext, HttpMethod.Post,
            $"api/events/{eventId}/enrolments", new { categoryId }, ct);
        TempData["Notice"] = result.Successful ? "You have enrolled successfully." :
            result.Error ?? "Enrolment was unsuccessful. Check that the race is open and has available spaces.";
        return result.Successful ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Details", "Home", new { id = eventId });
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        if (await Session(ct) is null) return RedirectToAction("Login", "Account");
        var (profile, status) = await api.GetAsync<RaceProfile>("api/users/me", Request, ct);
        if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");
        if (profile is null) return StatusCode(503, "Profile is temporarily unavailable.");
        return View(profile);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(RaceProfile input, CancellationToken ct)
    {
        if (await Session(ct) is null) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(input);
        var result = await api.MutateAsync(HttpContext, HttpMethod.Put, "api/users/me", input, ct);
        if (!result.Successful)
        {
            ModelState.AddModelError("", result.Error ?? "Profile update failed.");
            return View(input);
        }
        TempData["Notice"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(4_194_304)]
    public async Task<IActionResult> Picture(IFormFile? file, CancellationToken ct)
    {
        if (await Session(ct) is null) return RedirectToAction("Login", "Account");
        if (file is null || file.Length is < 1 or > 3 * 1024 * 1024)
        {
            TempData["Notice"] = "Choose a JPG or PNG smaller than 3 MB.";
            return RedirectToAction(nameof(Profile));
        }
        var result = await api.MutateAsync<object>(
            HttpContext, HttpMethod.Post, "api/users/me/picture", null, ct, image:file);
        TempData["Notice"] = result.Successful ? "Profile picture saved to private image storage." :
            result.Error ?? "Profile picture upload failed. Check Azure configuration.";
        return RedirectToAction(nameof(Profile));
    }
}