using System.ComponentModel.DataAnnotations;

namespace RaCeDay.Portal.Models;

public sealed class EventForm : IValidatableObject
{
    public int Id { get; set; }
    [Required, StringLength(150)] public string EventName { get; set; } = "";
    [StringLength(500)] public string? Description { get; set; }
    [Required, StringLength(200)] public string Location { get; set; } = "";
    [Required, RegularExpression("^(Run|Walk|Cycle)$")] public string EventType { get; set; } = "Run";
    [Required, RegularExpression("^(Planned|Open|Closed|Completed|Cancelled)$")]
    public string Status { get; set; } = "Planned";
    [DataType(DataType.Date)] public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
    [DataType(DataType.Date)] public DateOnly ClosingDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EventDate == default || ClosingDate == default || ClosingDate > EventDate)
            yield return new("Closing date cannot be after the event.", [nameof(ClosingDate)]);
    }
    public static EventForm From(RaceEvent e) => new()
    {
        Id=e.Id,EventName=e.EventName,Description=e.Description,Location=e.Location,
        EventType=e.EventType,Status=e.Status,EventDate=e.EventDate,ClosingDate=e.ClosingDate
    };
}

public sealed class CategoryForm
{
    public int Id { get; set; }
    public int EventId { get; set; }
    [Required, StringLength(100)] public string CategoryName { get; set; } = "";
    [Range(0.01, 9999.99)] public decimal DistanceKm { get; set; }
    [Range(0, 99999999.99)] public decimal EntryFee { get; set; }
    [Range(1, 1000000)] public int MaxParticipants { get; set; } = 100;
}
public sealed class ResultForm : IValidatableObject
{
    public int EventId { get; set; }
    public int EnrolmentId { get; set; }
    public string? EventName { get; set; }
    public string? ParticipantName { get; set; }
    [DataType(DataType.Time)] public TimeOnly? StartTime { get; set; }
    [DataType(DataType.Time)] public TimeOnly? FinishTime { get; set; }
    [Range(1, int.MaxValue)] public int? OverallPosition { get; set; }
    [Required, RegularExpression("^(Finished|DNS|DNF|DSQ)$")]
    public string ResultStatus { get; set; } = "Finished";

    // Mirror the API rules so organisers see useful errors before an HTTP request.
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ResultStatus == "Finished" &&
            (StartTime is null || FinishTime is null || OverallPosition is null))
            yield return new("A finisher needs start time, finish time and overall position.");
        if (StartTime is not null && FinishTime is not null && FinishTime < StartTime)
            yield return new("Finish time cannot precede start time.", [nameof(FinishTime)]);
    }
}
public sealed class RaceProfile
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    [Required, StringLength(60)] public string FirstName { get; set; } = "";
    [Required, StringLength(60)] public string LastName { get; set; } = "";
    [StringLength(30)] public string? Gender { get; set; }
    [Phone, StringLength(30)] public string? PhoneNumber { get; set; }
    [StringLength(100)] public string? OrganiserName { get; set; }
    [DataType(DataType.Date)] public DateOnly? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
}
public sealed record OrganiserEntriesPage(RaceEvent Event, IReadOnlyList<RaceEntry> Entries);
public sealed record CategoriesPage(RaceEvent Event, IReadOnlyList<RaceCategory> Categories);
