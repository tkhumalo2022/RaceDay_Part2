using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Controllers;

[ApiController, Route("api"), Authorize, AutoValidateAntiforgeryToken]
public class RaceApiController(RaceWorkflow workflow) : ControllerBase
{
    [HttpGet("antiforgery")]
    public IActionResult Antiforgery([FromServices] IAntiforgery antiforgery) =>
        Ok(new { requestToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
    [HttpGet("events")]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> Events() => Ok(await workflow.Events(User));
    [HttpGet("organiser/events"), Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> OwnEvents() => Ok(await workflow.Events(User, true));
    [HttpGet("events/{id:int}")]
    public async Task<ActionResult<EventDto>> Event(int id) => Ok(await workflow.Event(User, id));
    [HttpPost("events"), Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventDto), 201)]
    public async Task<ActionResult<EventDto>> Create(EventInput input)
    {
        var item = await workflow.WriteEvent(User, null, input);
        return CreatedAtAction(nameof(Event), new { id = item.Id }, item);
    }
    [HttpPut("events/{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<ActionResult<EventDto>> Update(int id, EventInput input) => Ok(await workflow.WriteEvent(User, id, input));
    [HttpDelete("events/{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id) { await workflow.DeleteEvent(User, id); return NoContent(); }
    [HttpGet("events/{eventId:int}/categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Categories(int eventId) => Ok(await workflow.Categories(User, eventId));
    [HttpPost("events/{eventId:int}/categories"), Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(int eventId, CategoryInput input)
    {
        var item = await workflow.WriteCategory(User, eventId, null, input);
        return Created($"/api/events/{eventId}/categories", item);
    }
    [HttpPut("categories/{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, CategoryInput input) =>
        Ok(await workflow.WriteCategory(User, await workflow.CategoryEvent(id), id, input));
    [HttpDelete("categories/{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(int id) { await workflow.DeleteCategory(User, id); return NoContent(); }
    [HttpPost("events/{eventId:int}/enrolments"), Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(Enrolment), 201)]
    public async Task<IActionResult> Enrol(int eventId, EnrolmentInput input)
    {
        var entry = await workflow.Enrol(User, eventId, input);
        return Created("/api/enrolments/me", new { entry.Id, entry.EventId, entry.CategoryId, entry.RaceNumber, entry.Status });
    }
    [HttpGet("enrolments/me"), Authorize(Roles = "Participant")]
    public async Task<ActionResult<IReadOnlyList<EntryDto>>> Mine() => Ok(await workflow.MyEntries(User));
    [HttpGet("events/{eventId:int}/enrolments"), Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IReadOnlyList<EntryDto>>> Entries(int eventId) => Ok(await workflow.EventEntries(User, eventId));
    [HttpPost("enrolments/{enrolmentId:int}/results"), Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(RaceResult), 201)]
    public async Task<ActionResult<RaceResult>> Result(int enrolmentId, ResultInput input) =>
        Created("/api/results/me", await workflow.RecordResult(User, enrolmentId, input));
    [HttpGet("enrolments/{enrolmentId:int}/payments")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> Payments(int enrolmentId) =>
        Ok(await workflow.Payments(User, enrolmentId));
    [HttpPost("enrolments/{enrolmentId:int}/payments"), Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(PaymentDto), 201)]
    public async Task<ActionResult<PaymentDto>> RecordPayment(int enrolmentId, PaymentInput input) =>
        Created($"/api/enrolments/{enrolmentId}/payments", await workflow.RecordPayment(User, enrolmentId, input));
    [HttpGet("results/me"), Authorize(Roles = "Participant")]
    public async Task<ActionResult<IReadOnlyList<EntryDto>>> MyResults() => Ok(await workflow.MyEntries(User, true));
    [HttpGet("events/{eventId:int}/results"), Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IReadOnlyList<EntryDto>>> Results(int eventId) => Ok(await workflow.EventEntries(User, eventId, true));
}
