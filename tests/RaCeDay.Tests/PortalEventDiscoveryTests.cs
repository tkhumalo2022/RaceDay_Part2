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

    [Fact]
    public void DateWindowsIncludeTodayAndRespectExactCalendarBoundaries()
    {
        var days = new[] { 0, 6, 7, 29, 30 };
        var races = days.Select((offset, i) => new RaceEvent(i + 1, "Race", null,
            Today.AddDays(offset), "Durban", "Run", "Open", Today, false));
        var week = EventDiscovery.Find(races, null, null, Today, "7");
        var month = EventDiscovery.Find(races, null, null, Today, "30");
        Assert.Equal([1, 2], week.Events.Select(x => x.Id).ToArray());
        Assert.Equal([1, 2, 3, 4], month.Events.Select(x => x.Id).ToArray());
        Assert.Equal("7", week.Window);
        Assert.Equal("30", month.Window);
    }

    [Fact]
    public void DateWindowCombinesWithSearchAndActivityFilter()
    {
        var races = new[]
        {
            new RaceEvent(1, "Durban Sprint", null, Today.AddDays(3), "Durban", "Run", "Open", Today, false),
            new RaceEvent(2, "Durban Ride", null, Today.AddDays(3), "Durban", "Cycle", "Open", Today, false),
            new RaceEvent(3, "Durban Marathon", null, Today.AddDays(18), "Durban", "Run", "Open", Today, false)
        };
        var result = EventDiscovery.Find(races, "run", "durban", Today, "7");
        Assert.Equal([1], result.Events.Select(x => x.Id).ToArray());
        Assert.Equal("Run", result.Filter);
        Assert.Equal("durban", result.Query);
    }

    [Fact]
    public void InvalidDateWindowFallsBackToAnyUpcomingDate()
    {
        var result = EventDiscovery.Find(Races, null, null, Today, "forever");
        Assert.Null(result.Window);
        Assert.Equal([1, 2], result.Events.Select(x => x.Id).ToArray());
    }

    [Fact]
    public void EnrolButtonMatchesEventDatesStatusAndCapacity()
    {
        var race = Races[0] with { EventDate = Today.AddDays(3) };
        var category = new RaceCategory(1, race.Id, "5 km", 5, 50, 10, 2);

        Assert.True(EventDiscovery.CanEnrol(race, category, Today));
        Assert.False(EventDiscovery.CanEnrol(race with { EventDate = Today.AddDays(-1) }, category, Today));
        Assert.False(EventDiscovery.CanEnrol(race with { ClosingDate = Today.AddDays(-1) }, category, Today));
        Assert.False(EventDiscovery.CanEnrol(race with { Status = "Closed" }, category, Today));
        Assert.False(EventDiscovery.CanEnrol(race, category with { CurrentParticipants = 10 }, Today));
    }
}
