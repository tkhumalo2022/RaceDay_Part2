using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RaCeDay.Data;
using RaCeDay.Models;

namespace RaCeDay.Services;

public class WorkflowException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}

// Shared by MVC and REST so ownership and business rules cannot diverge.
public class RaceWorkflow(ApplicationDbContext db)
{
    private static string UserId(ClaimsPrincipal actor) => actor.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new WorkflowException(401, "Sign in to continue.");
    private static string Role(ClaimsPrincipal actor, string role)
    {
        var id = UserId(actor);
        if (!actor.IsInRole(role)) throw new WorkflowException(403, "This action is not available for your role.");
        return id;
    }
    private static void Validate(object input)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), errors, true))
            throw new WorkflowException(400, string.Join(" ", errors.Select(e => e.ErrorMessage)));
    }
    private static void Own(RaceEvent item, string id)
    {
        if (item.OrganiserId != id) throw new WorkflowException(403, "Only the event organiser can manage this event.");
    }
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2));
    private async Task<RaceEvent> FindEvent(int id) => await db.Events.SingleOrDefaultAsync(x => x.Id == id)
        ?? throw new WorkflowException(404, "Event not found.");
    // All writes for an event take this SQL Server update lock in a transaction.
    // This includes capacity edits, enrolments and deletes, preventing check/write races.
    private async Task<RaceEvent> LockEvent(int id) => await db.Events
        .FromSqlInterpolated($"SELECT * FROM [Event] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
        .SingleOrDefaultAsync() ?? throw new WorkflowException(404, "Event not found.");
    private async Task Save()
    {
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 or 547 })
        { throw new WorkflowException(409, "The change conflicts with an existing entry or related data. Refresh and try again."); }
    }
    private static EventDto EventView(RaceEvent e, string actor) => new(e.Id, e.EventName, e.Description, e.EventDate, e.Location, e.EventType, e.Status, e.ClosingDate, e.OrganiserId == actor);
    public async Task<IReadOnlyList<EventDto>> Events(ClaimsPrincipal actor, bool mine = false)
    {
        var id = UserId(actor);
        if (mine) Role(actor, "Organiser");
        var rows = await db.Events.AsNoTracking().Where(e => !mine || e.OrganiserId == id).OrderBy(e => e.EventDate).ThenBy(e => e.Id).ToListAsync();
        return rows.Select(e => EventView(e, id)).ToList();
    }
    public async Task<EventDto> Event(ClaimsPrincipal actor, int id) => EventView(await FindEvent(id), UserId(actor));
    public async Task<EventDto> WriteEvent(ClaimsPrincipal actor, int? id, EventInput input)
    {
        var userId = Role(actor, "Organiser"); Validate(input);
        await using var tx = await db.Database.BeginTransactionAsync();
        var item = id is int key ? await LockEvent(key) : new RaceEvent { OrganiserId = userId };
        Own(item, userId);
        item.EventName = input.EventName.Trim(); item.Description = input.Description?.Trim();
        item.EventDate = input.EventDate; item.ClosingDate = input.ClosingDate; item.Location = input.Location.Trim();
        item.EventType = input.EventType; item.Status = input.Status;
        if (id is null) db.Events.Add(item);
        await Save(); await tx.CommitAsync();
        return EventView(item, userId);
    }
    public async Task DeleteEvent(ClaimsPrincipal actor, int id)
    {
        var userId = Role(actor, "Organiser");
        await using var tx = await db.Database.BeginTransactionAsync();
        var item = await LockEvent(id); Own(item, userId);
        if (await db.Enrolments.AnyAsync(e => e.EventId == id)) throw new WorkflowException(409, "Events with entries cannot be deleted. Close or cancel the event instead.");
        db.Categories.RemoveRange(await db.Categories.Where(c => c.EventId == id).ToListAsync());
        db.Events.Remove(item); await Save(); await tx.CommitAsync();
    }
    public async Task<IReadOnlyList<CategoryDto>> Categories(ClaimsPrincipal actor, int eventId)
    {
        UserId(actor); await FindEvent(eventId);
        return await db.Categories.AsNoTracking().Where(c => c.EventId == eventId).OrderBy(c => c.DistanceKm)
            .Select(c => new CategoryDto(c.Id, c.EventId, c.CategoryName, c.DistanceKm, c.EntryFee, c.MaxParticipants,
                c.CurrentParticipants)).ToListAsync();
    }
    public async Task<CategoryDto> WriteCategory(ClaimsPrincipal actor, int eventId, int? id, CategoryInput input)
    {
        var userId = Role(actor, "Organiser"); Validate(input);
        await using var tx = await db.Database.BeginTransactionAsync();
        Own(await LockEvent(eventId), userId);
        var item = id is int key ? await db.Categories.SingleOrDefaultAsync(c => c.Id == key && c.EventId == eventId)
            ?? throw new WorkflowException(404, "Category not found.") : new RaceCategory { EventId = eventId };
        var count = id is null ? 0 : await db.Categories.Where(c => c.Id == id).Select(c => c.CurrentParticipants).SingleAsync();
        if (input.MaxParticipants < count) throw new WorkflowException(409, "Capacity cannot be less than existing entries.");
        var name = input.CategoryName.Trim();
        if (await db.Categories.AnyAsync(c => c.EventId == eventId && c.Id != (id ?? 0) && c.CategoryName == name))
            throw new WorkflowException(409, "Category names must be unique within an event.");
        item.CategoryName = name; item.DistanceKm = input.DistanceKm; item.EntryFee = input.EntryFee; item.MaxParticipants = input.MaxParticipants;
        if (id is null) db.Categories.Add(item);
        await Save(); await tx.CommitAsync();
        return new(item.Id, eventId, name, item.DistanceKm, item.EntryFee, item.MaxParticipants, count);
    }
    public async Task<int> CategoryEvent(int id) => await db.Categories.Where(c => c.Id == id).Select(c => (int?)c.EventId).SingleOrDefaultAsync()
        ?? throw new WorkflowException(404, "Category not found.");
    public async Task DeleteCategory(ClaimsPrincipal actor, int id)
    {
        var userId = Role(actor, "Organiser"); var eventId = await CategoryEvent(id);
        await using var tx = await db.Database.BeginTransactionAsync();
        Own(await LockEvent(eventId), userId);
        var item = await db.Categories.SingleOrDefaultAsync(c => c.Id == id) ?? throw new WorkflowException(404, "Category not found.");
        if (await db.Enrolments.AnyAsync(e => e.CategoryId == id)) throw new WorkflowException(409, "Categories with entries cannot be deleted.");
        db.Categories.Remove(item); await Save(); await tx.CommitAsync();
    }
    public async Task<Enrolment> Enrol(ClaimsPrincipal actor, int eventId, EnrolmentInput input)
    {
        var userId = Role(actor, "Participant"); Validate(input);
        await using var tx = await db.Database.BeginTransactionAsync();
        var race = await LockEvent(eventId);
        if (race.Status != "Open" || race.ClosingDate < Today || race.EventDate < Today)
            throw new WorkflowException(409, "This event is not open for enrolment.");
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == input.CategoryId && c.EventId == eventId)
            ?? throw new WorkflowException(400, "Choose a category belonging to this event.");
        if (await db.Enrolments.AnyAsync(e => e.CategoryId == category.Id && e.ParticipantId == userId))
            throw new WorkflowException(409, "You already have an entry for this race category.");
        if (category.CurrentParticipants >= category.MaxParticipants)
            throw new WorkflowException(409, "This category is full.");
        var entry = new Enrolment { EventId = eventId, CategoryId = category.Id, ParticipantId = userId };
        db.Enrolments.Add(entry); await Save(); await tx.CommitAsync(); return entry;
    }
    private IQueryable<EntryDto> EntriesQuery(IQueryable<Enrolment> entries) =>
        from e in entries
        join race in db.Events on e.EventId equals race.Id
        join category in db.Categories on e.CategoryId equals category.Id
        join participant in db.Users on e.ParticipantId equals participant.Id
        join result in db.Results on e.Id equals result.EnrolmentId into results
        from r in results.DefaultIfEmpty()
        orderby e.Id descending
        select new EntryDto(e.Id, e.EventId, e.CategoryId, race.EventName, category.CategoryName, e.RaceNumber, e.Status,
            r == null ? null : r.ResultStatus, r == null ? null : r.StartTime, r == null ? null : r.FinishTime, r == null ? null : r.OverallPosition,
            participant.FirstName + " " + participant.LastName);
    public async Task<IReadOnlyList<EntryDto>> MyEntries(ClaimsPrincipal actor, bool resultsOnly = false)
    {
        var id = Role(actor, "Participant");
        return await EntriesQuery(db.Enrolments.AsNoTracking().Where(e => e.ParticipantId == id && (!resultsOnly || db.Results.Any(r => r.EnrolmentId == e.Id)))).ToListAsync();
    }
    public async Task<IReadOnlyList<EntryDto>> EventEntries(ClaimsPrincipal actor, int eventId, bool resultsOnly = false)
    {
        Own(await FindEvent(eventId), Role(actor, "Organiser"));
        return await EntriesQuery(db.Enrolments.AsNoTracking().Where(e => e.EventId == eventId && (!resultsOnly || db.Results.Any(r => r.EnrolmentId == e.Id)))).ToListAsync();
    }
    public async Task<int> OwnedEntryEvent(ClaimsPrincipal actor, int id)
    {
        var userId = Role(actor, "Organiser");
        var entry = await db.Enrolments.AsNoTracking().SingleOrDefaultAsync(e => e.Id == id) ?? throw new WorkflowException(404, "Entry not found.");
        Own(await FindEvent(entry.EventId), userId); return entry.EventId;
    }
    public async Task<RaceResult> RecordResult(ClaimsPrincipal actor, int enrolmentId, ResultInput input)
    {
        Validate(input); var eventId = await OwnedEntryEvent(actor, enrolmentId);
        await using var tx = await db.Database.BeginTransactionAsync();
        var race = await LockEvent(eventId); Own(race, Role(actor, "Organiser"));
        if (race.Status is not ("Closed" or "Completed")) throw new WorkflowException(409, "Close enrolment before recording results.");
        if (await db.Results.AnyAsync(r => r.EnrolmentId == enrolmentId)) throw new WorkflowException(409, "This entry already has a result.");
        var entry = await db.Enrolments.SingleAsync(e => e.Id == enrolmentId);
        var result = new RaceResult { EnrolmentId = enrolmentId, StartTime = input.StartTime, FinishTime = input.FinishTime,
            OverallPosition = input.OverallPosition, ResultStatus = input.ResultStatus };
        entry.Status = "Completed"; db.Results.Add(result); await Save(); await tx.CommitAsync(); return result;
    }

    private static PaymentDto PaymentView(PaymentAttempt p) =>
        new(p.Id, p.EnrolmentId, p.Amount, p.PaymentMethod, p.Status, p.PaymentDate, p.TransactionReference);

    private async Task<Enrolment> PaymentEntry(ClaimsPrincipal actor, int enrolmentId, bool participantOnly)
    {
        var id = UserId(actor);
        var entry = await db.Enrolments.AsNoTracking().SingleOrDefaultAsync(e => e.Id == enrolmentId)
            ?? throw new WorkflowException(404, "Enrolment not found.");
        if (entry.ParticipantId == id && actor.IsInRole("Participant")) return entry;
        if (!participantOnly && actor.IsInRole("Organiser") &&
            await db.Events.AnyAsync(e => e.Id == entry.EventId && e.OrganiserId == id)) return entry;
        throw new WorkflowException(403, "You cannot access payments for this enrolment.");
    }

    public async Task<IReadOnlyList<PaymentDto>> Payments(ClaimsPrincipal actor, int enrolmentId)
    {
        await PaymentEntry(actor, enrolmentId, false);
        var rows = await db.Payments.AsNoTracking().Where(p => p.EnrolmentId == enrolmentId)
            .OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id).ToListAsync();
        return rows.Select(PaymentView).ToList();
    }

    public async Task<decimal> ExpectedPaymentAmount(ClaimsPrincipal actor, int enrolmentId)
    {
        Role(actor, "Participant");
        var entry = await PaymentEntry(actor, enrolmentId, true);
        return await db.Categories.Where(c => c.Id == entry.CategoryId && c.EventId == entry.EventId)
            .Select(c => c.EntryFee).SingleAsync();
    }

    public async Task<PaymentDto> RecordPayment(ClaimsPrincipal actor, int enrolmentId, PaymentInput input)
    {
        Role(actor, "Participant");
        Validate(input);
        var entry = await PaymentEntry(actor, enrolmentId, true);
        await using var tx = await db.Database.BeginTransactionAsync();
        await LockEvent(entry.EventId);
        if (entry.Status != "Registered")
            throw new WorkflowException(409, "This enrolment no longer accepts payment attempts.");

        var expected = await db.Categories.Where(c => c.Id == entry.CategoryId && c.EventId == entry.EventId)
            .Select(c => c.EntryFee).SingleAsync();
        if (input.Amount != expected)
            throw new WorkflowException(400, "Payment amount must equal the race category entry fee.");
        if (await db.Payments.AnyAsync(p => p.EnrolmentId == enrolmentId && p.Status == "Paid"))
            throw new WorkflowException(409, "This enrolment already has a successful payment.");

        var reference = input.TransactionReference?.Trim();
        if (reference == "") reference = null;
        if (reference is not null && await db.Payments.AnyAsync(p => p.TransactionReference == reference))
            throw new WorkflowException(409, "This payment reference has already been recorded.");

        // User-submitted attempts are always Pending; clients cannot self-declare Paid.
        var payment = new PaymentAttempt { EnrolmentId = enrolmentId, Amount = expected,
            PaymentMethod = input.PaymentMethod, TransactionReference = reference, Status = "Pending" };
        db.Payments.Add(payment);
        await Save();
        await tx.CommitAsync();
        return PaymentView(payment);
    }
}
