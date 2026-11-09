using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Portal.Models;
using RaCeDay.Portal.Services;

namespace RaCeDay.Portal.Controllers;
public sealed class AccountController(RaceDayApiClient api) : Controller
{
    [HttpGet] public IActionResult Login() => View(new LoginInput());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(input);
        try
        {
            var status = await api.AuthAsync("login", input, HttpContext, ct);
            if (status == HttpStatusCode.OK) return RedirectToAction("Index", "Home");
            ModelState.AddModelError("", status switch
            {
                (HttpStatusCode)423 => "Account temporarily locked.",
                HttpStatusCode.Forbidden => "This account cannot sign in yet.",
                _ => "The login details were not accepted."
            });
        }
        catch (HttpRequestException) { ModelState.AddModelError("", "The API is offline."); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        { ModelState.AddModelError("", "The API did not respond."); }
        return View(input);
    }
    [HttpGet] public IActionResult Register() => View(new RegisterInput());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(input);
        try
        {
            var status = await api.AuthAsync("register", input, HttpContext, ct);
            if (status == HttpStatusCode.Created) return RedirectToAction("Index", "Home");
            ModelState.AddModelError("", status == HttpStatusCode.Conflict
                ? "This email is already registered."
                : "Registration failed. Check your details.");
        }
        catch (HttpRequestException) { ModelState.AddModelError("", "The API is offline."); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        { ModelState.AddModelError("", "The API did not respond."); }
        return View(input);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        try
        {
            var status = await api.AuthAsync("logout", new { }, HttpContext, ct);
            if (status is HttpStatusCode.NoContent or HttpStatusCode.Unauthorized)
                return RedirectToAction("Login");
        }
        catch (HttpRequestException) { }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested) { }
        TempData["Error"] = "Sign-out could not be confirmed. Try again.";
        return RedirectToAction("Index", "Home");
    }
}
