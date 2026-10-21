using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaCeDay.Data;
using RaCeDay.Models;

namespace RaCeDay.Controllers;

[Authorize]
public class DashboardController(ApplicationDbContext db) : Controller
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

    // The administrator gets totals, not private participant details or control over
    // events owned by someone else.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin(CancellationToken cancellationToken)
    {
        var summary = new AdminDashboardViewModel
        {
            Users = await db.Users.CountAsync(cancellationToken),
            Events = await db.Events.CountAsync(cancellationToken),
            Categories = await db.Categories.CountAsync(cancellationToken),
            Enrolments = await db.Enrolments.CountAsync(cancellationToken),
            Results = await db.Results.CountAsync(cancellationToken),
            Payments = await db.Payments.CountAsync(cancellationToken)
        };
        return View("Admin", summary);
    }
}
