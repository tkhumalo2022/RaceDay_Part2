using System.ComponentModel.DataAnnotations;

namespace RaCeDay.Models;

public class RaceEvent
{
    public int Id { get; set; }
    public string OrganiserId { get; set; } = "";
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = "";
    public string EventType { get; set; } = "Run";
    public string Status { get; set; } = "Planned";
    public DateOnly ClosingDate { get; set; }
}
public class RaceCategory
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
}
public class Enrolment
{
    public int Id { get; set; }
    public string ParticipantId { get; set; } = "";
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public string RaceNumber { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Registered";
}
public class RaceResult
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? FinishTime { get; set; }
    public int? OverallPosition { get; set; }
    public string ResultStatus { get; set; } = "Finished";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

public class EventInput : IValidatableObject
{
    [Required, StringLength(150)] public string EventName { get; set; } = "";
    [StringLength(500)] public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    [Required, StringLength(200)] public string Location { get; set; } = "";
    [Required, RegularExpression("^(Run|Walk|Cycle)$")] public string EventType { get; set; } = "Run";
    [Required, RegularExpression("^(Planned|Open|Closed|Completed|Cancelled)$")] public string Status { get; set; } = "Planned";
    public DateOnly ClosingDate { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EventDate == default || ClosingDate == default || ClosingDate > EventDate)
            yield return new("Provide valid dates with closing date on or before event date.", [nameof(ClosingDate)]);
    }
}
public class CategoryInput
{
    [Required, StringLength(100)] public string CategoryName { get; set; } = "";
    [Range(typeof(decimal), "0.01", "9999.99", ParseLimitsInInvariantCulture = true)] public decimal DistanceKm { get; set; }
    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true)] public decimal EntryFee { get; set; }
    [Range(1, 1000000)] public int MaxParticipants { get; set; }
}
public class EnrolmentInput
{
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
}
public class ResultInput : IValidatableObject
{
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? FinishTime { get; set; }
    [Range(1, int.MaxValue)] public int? OverallPosition { get; set; }
    [Required, RegularExpression("^(Finished|DNS|DNF|DSQ)$")] public string ResultStatus { get; set; } = "Finished";
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ResultStatus == "Finished" && (StartTime is null || FinishTime is null || OverallPosition is null))
            yield return new("Finished results require start time, finish time and position.");
        if (FinishTime < StartTime) yield return new("Finish time cannot precede start time.");
    }
}

public record EventDto(int Id, string EventName, string? Description, DateOnly EventDate, string Location, string EventType, string Status, DateOnly ClosingDate, bool IsOwner);
public record CategoryDto(int Id, int EventId, string CategoryName, decimal DistanceKm, decimal EntryFee, int MaxParticipants, int CurrentParticipants);
public record EntryDto(int Id, int EventId, int CategoryId, string EventName, string CategoryName, string RaceNumber, string Status, string? ResultStatus, TimeOnly? StartTime, TimeOnly? FinishTime, int? OverallPosition);
public record EventDetails(EventDto Event, IReadOnlyList<CategoryDto> Categories);
