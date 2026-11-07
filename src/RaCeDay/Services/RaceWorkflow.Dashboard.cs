using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RaCeDay.Models;

namespace RaCeDay.Services;
public partial class RaceWorkflow
{
    public async Task<IReadOnlyList<OrganiserEventSummary>> EventSummary(ClaimsPrincipal actor)
    {
        var userId = actor.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) throw new WorkflowException(401, "Sign in.");
        if (!actor.IsInRole("Organiser")) throw new WorkflowException(403, "Organiser access required.");
        return await db.Events.AsNoTracking()
            .Where(e => e.OrganiserId == userId)
            .Select(e => new OrganiserEventSummary(
                e.Id, e.EventName, e.EventDate, e.Status,
                db.Enrolments.Count(en => en.EventId == e.Id),
                db.Results.Count(res => res.ResultStatus == "Finished"
                    && db.Enrolments.Any(en => en.Id == res.EnrolmentId && en.EventId == e.Id))))
            .OrderBy(e => e.EventDate)
            .ToListAsync();
    }
}