namespace RaCeDay.Models;
public sealed record OrganiserEventSummary(
    int EventId, string EventName, DateOnly EventDate, string Status,
    int TotalEnrolments, int Finishers);