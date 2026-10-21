using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Controllers;

[Authorize, AutoValidateAntiforgeryToken]
public class EntriesController(RaceWorkflow workflow) : Controller
{
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Index() => View(await workflow.MyEntries(User));
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Results() => View("Index", await workflow.MyEntries(User, true));
    [HttpPost, Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(int eventId, EnrolmentInput model)
    {
        if (!ModelState.IsValid) throw new WorkflowException(400, "Choose a valid category.");
        await workflow.Enrol(User, eventId, model); return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Event(int id)
    {
        ViewData["RecordResults"] = true;
        return View("Index", await workflow.EventEntries(User, id));
    }
    [HttpGet, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Result(int id)
    {
        await workflow.OwnedEntryEvent(User, id); return View(new ResultInput());
    }
    [HttpPost, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Result(int id, ResultInput model)
    {
        if (!ModelState.IsValid) return View(model);
        var eventId = await workflow.OwnedEntryEvent(User, id);
        await workflow.RecordResult(User, id, model); return RedirectToAction(nameof(Event), new { id = eventId });
    }
}
