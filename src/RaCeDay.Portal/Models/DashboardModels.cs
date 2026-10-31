namespace RaCeDay.Portal.Models;
public sealed record RaceEntry(int Id, int EventId, int CategoryId, string EventName,
    string CategoryName, string RaceNumber, string Status, string? ResultStatus,
    TimeOnly? StartTime, TimeOnly? FinishTime, int? OverallPosition, string ParticipantName, int? TotalFinishers = null, DateOnly? EventDate = null);
public sealed record OrganiserStat(int EventId, string EventName, DateOnly EventDate,
    string Status, int TotalEnrolments, int Finishers);
public sealed record DashboardPage(string Role, IReadOnlyList<RaceEvent> Events,
    IReadOnlyList<RaceEntry> Entries, IReadOnlyList<RaceEntry> Results, string? Error)
{
    public IReadOnlyList<OrganiserStat> Summary { get; init; } = [];
}
