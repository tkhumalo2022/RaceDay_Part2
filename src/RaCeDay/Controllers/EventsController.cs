using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Controllers;

[Authorize, AutoValidateAntiforgeryToken]
public class EventsController(RaceWorkflow workflow) : Controller
{
    public async Task<IActionResult> Index() => View(await workflow.Events(User));
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Mine() => View("Index", await workflow.Events(User, true));
    public async Task<IActionResult> Details(int id) => View(new EventDetails(await workflow.Event(User, id), await workflow.Categories(User, id)));
    [HttpGet, Authorize(Roles = "Organiser")]
    public IActionResult Create() => View("Edit", new EventInput { EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), ClosingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)) });
    [HttpPost, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(EventInput model)
    {
        if (!ModelState.IsValid) return View("Edit", model);
        var item = await workflow.WriteEvent(User, null, model);
        return RedirectToAction(nameof(Details), new { id = item.Id });
    }
    [HttpGet, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await workflow.Event(User, id);
        if (!e.IsOwner) return Forbid();
        return View(new EventInput { EventName = e.EventName, Description = e.Description, EventDate = e.EventDate,
            ClosingDate = e.ClosingDate, Location = e.Location, Status = e.Status, EventType = e.EventType });
    }
    [HttpPost, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Edit(int id, EventInput model)
    {
        if (!ModelState.IsValid) return View(model);
        await workflow.WriteEvent(User, id, model); return RedirectToAction(nameof(Details), new { id });
    }
    [HttpPost, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id) { await workflow.DeleteEvent(User, id); return RedirectToAction(nameof(Mine)); }
}
