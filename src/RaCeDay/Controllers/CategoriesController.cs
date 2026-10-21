using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Controllers;

[Authorize(Roles = "Organiser"), AutoValidateAntiforgeryToken]
public class CategoriesController(RaceWorkflow workflow) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int eventId)
    {
        if (!(await workflow.Event(User, eventId)).IsOwner) return Forbid();
        return View("Edit", new CategoryInput { DistanceKm = 5, MaxParticipants = 100 });
    }
    [HttpPost]
    public async Task<IActionResult> Create(int eventId, CategoryInput model)
    {
        if (!ModelState.IsValid) return View("Edit", model);
        await workflow.WriteCategory(User, eventId, null, model);
        return RedirectToAction("Details", "Events", new { id = eventId });
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var eventId = await workflow.CategoryEvent(id);
        if (!(await workflow.Event(User, eventId)).IsOwner) return Forbid();
        var c = (await workflow.Categories(User, eventId)).Single(c => c.Id == id);
        return View(new CategoryInput { CategoryName = c.CategoryName, DistanceKm = c.DistanceKm, EntryFee = c.EntryFee, MaxParticipants = c.MaxParticipants });
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, CategoryInput model)
    {
        if (!ModelState.IsValid) return View(model);
        var eventId = await workflow.CategoryEvent(id);
        await workflow.WriteCategory(User, eventId, id, model);
        return RedirectToAction("Details", "Events", new { id = eventId });
    }
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var eventId = await workflow.CategoryEvent(id); await workflow.DeleteCategory(User, id);
        return RedirectToAction("Details", "Events", new { id = eventId });
    }
}
