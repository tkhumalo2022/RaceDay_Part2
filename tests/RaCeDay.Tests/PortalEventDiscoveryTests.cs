using RaCeDay.Portal.Models;
using RaCeDay.Portal.Services;

namespace RaCeDay.Tests;

public class PortalEventDiscoveryTests
{
    private static readonly DateOnly Today = new(2026, 11, 1);
    private static readonly RaceEvent[] Races =
    [
        new(1, "Durban Run", null, new DateOnly(2026, 12, 1), "Durban", "Run", "Open", Today, false),
        new(2, "Coastal Cycle", null, new DateOnly(2026, 12, 2), "Richards Bay", "Cycle", "Planned", Today, false),
        new(3, "Old Walk", null, new DateOnly(2026, 10, 1), "Durban", "Walk", "Open", Today, false),
        new(4, "Cancelled Run", null, new DateOnly(2026, 12, 3), "Durban", "Run", "Cancelled", Today, false)
    ];

    [Fact]
    public void OnlyUpcomingOpenOrPlannedEventsAreShown()
    {
        var result = EventDiscovery.Find(Races, null, null, Today);
        Assert.Equal([1, 2], result.Events.Select(x => x.Id).ToArray());
    }

    [Fact]
    public void LocationAndEventNameSearchIgnoreCase()
    {
        Assert.Equal([2], EventDiscovery.Find(Races, null, "richards", Today)
            .Events.Select(x => x.Id).ToArray());
        Assert.Equal([1], EventDiscovery.Find(Races, null, "DURBAN RUN", Today)
            .Events.Select(x => x.Id).ToArray());
    }

    [Fact]
    public void ActivityAndTrimmedQueryAreAppliedTogether()
    {
        var result = EventDiscovery.Find(Races, "rUn", "  durban  ", Today);
        Assert.Equal("Run", result.Filter);
        Assert.Equal("durban", result.Query);
        Assert.Equal([1], result.Events.Select(x => x.Id).ToArray());
    }

    [Fact]
    public void UnknownActivityIsIgnoredAndLongSearchIsBounded()
    {
        var result = EventDiscovery.Find(Races, "invalid", new string('x', 110), Today);
        Assert.Null(result.Filter);
        Assert.Equal(100, result.Query?.Length);
        Assert.Empty(result.Events);
    }
}
