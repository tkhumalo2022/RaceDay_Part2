using RaCeDay.Portal.Models;

namespace RaCeDay.Portal.Services;

// Keeps the MVC discovery rules testable without needing a SQL Server connection.
public static class EventDiscovery
{
    public static (List<RaceEvent> Events, string? Filter, string? Query) Find(
        IEnumerable<RaceEvent> events, string? type, string? query, DateOnly today)
    {
        var filter = new[] { "Run", "Walk", "Cycle" }
            .FirstOrDefault(x => x.Equals(type, StringComparison.OrdinalIgnoreCase));
        var search = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        if (search?.Length > 100) search = search[..100];

        var matching = events.Where(x =>
            x.EventDate >= today &&
            (x.Status == "Open" || x.Status == "Planned") &&
            (filter is null || x.EventType.Equals(filter, StringComparison.OrdinalIgnoreCase)) &&
            (search is null ||
                x.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.Location.Contains(search, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.EventDate)
            .ToList();
        return (matching, filter, search);
    }
}
