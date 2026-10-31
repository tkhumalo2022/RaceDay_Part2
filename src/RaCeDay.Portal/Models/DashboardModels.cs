namespace RaCeDay.Portal.Models;
public sealed record RaceEntry(int Id, int EventId, int CategoryId, string EventName,
    string CategoryName, string RaceNumber, string Status, string? ResultStatus,
    TimeOnly? StartTime, TimeOnly? FinishTime, int? OverallPosition, string ParticipantName);
public sealed record DashboardPage(string Role, IReadOnlyList<RaceEvent> Events,
    IReadOnlyList<RaceEntry> Entries, IReadOnlyList<RaceEntry> Results, string? Error);
