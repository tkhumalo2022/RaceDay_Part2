using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaCeDay.Controllers;

[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Admin")) return RedirectToAction(nameof(Admin));
        if (User.IsInRole("Organiser")) return RedirectToAction(nameof(Organiser));
        if (User.IsInRole("Participant")) return RedirectToAction(nameof(Participant));
        return Forbid();
    }

    [Authorize(Roles = "Participant")]
    public IActionResult Participant() => View("Index", "Participant");

    [Authorize(Roles = "Organiser")]
    public IActionResult Organiser() => View("Index", "Organiser");

    [Authorize(Roles = "Admin")]
    public IActionResult Admin() => View("Index", "Admin");
}
