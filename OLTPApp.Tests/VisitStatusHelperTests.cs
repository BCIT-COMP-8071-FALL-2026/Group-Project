using OLTPApp.Models;
using OLTPApp.Services;

namespace OLTPApp.Tests;

// #59 (OLTP-17): Missed rule in docs/Query1-Contract.md
public class VisitStatusHelperTests
{
    private static readonly DateTime Start = new(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc);

    private static Visit NewVisit(VisitStatus status, TimeSpan? checkInAfterStart = null) => new()
    {
        ScheduledStart = Start,
        ActualCheckIn = checkInAfterStart is { } offset ? Start + offset : null,
        Status = status,
    };

    [Theory]
    [InlineData(59, VisitDisplayStatus.InProgress)]
    [InlineData(60, VisitDisplayStatus.InProgress)]
    [InlineData(61, VisitDisplayStatus.Missed)]
    public void CheckIn_AtMinutesAfterStart_IsMissedOnlyAfter60(int minutes, VisitDisplayStatus expected)
    {
        var visit = NewVisit(VisitStatus.InProgress, TimeSpan.FromMinutes(minutes));

        Assert.Equal(expected, VisitStatusHelper.GetDisplayStatus(visit, Start.AddHours(2)));
    }

    [Theory]
    [InlineData(59, VisitDisplayStatus.Scheduled)]
    [InlineData(60, VisitDisplayStatus.Missed)]
    [InlineData(61, VisitDisplayStatus.Missed)]
    public void NoCheckIn_AtMinutesAfterStart_IsMissedFrom60(int minutes, VisitDisplayStatus expected)
    {
        var visit = NewVisit(VisitStatus.Scheduled);

        Assert.Equal(expected, VisitStatusHelper.GetDisplayStatus(visit, Start.AddMinutes(minutes)));
    }

    [Fact]
    public void CheckIn_FractionOfASecondAfterCutoff_IsMissed()
    {
        var visit = NewVisit(VisitStatus.InProgress, TimeSpan.FromMinutes(60).Add(TimeSpan.FromMilliseconds(400)));

        Assert.Equal(VisitDisplayStatus.Missed, VisitStatusHelper.GetDisplayStatus(visit, Start.AddHours(2)));
    }

    [Fact]
    public void Cancelled_IsNeverMissed()
    {
        var visit = NewVisit(VisitStatus.Cancelled);

        Assert.Equal(VisitDisplayStatus.Cancelled, VisitStatusHelper.GetDisplayStatus(visit, Start.AddDays(1)));
    }

    [Fact]
    public void Completed_OnTime_IsCompleted()
    {
        var visit = NewVisit(VisitStatus.Completed, TimeSpan.FromMinutes(5));

        Assert.Equal(VisitDisplayStatus.Completed, VisitStatusHelper.GetDisplayStatus(visit, Start.AddDays(1)));
    }

    [Fact]
    public void Completed_WithCheckInAfterCutoff_IsMissed()
    {
        var visit = NewVisit(VisitStatus.Completed, TimeSpan.FromMinutes(75));

        Assert.Equal(VisitDisplayStatus.Missed, VisitStatusHelper.GetDisplayStatus(visit, Start.AddDays(1)));
    }

    [Fact]
    public void CheckIn_BeforeStart_CountsAsWithin()
    {
        var visit = NewVisit(VisitStatus.InProgress, TimeSpan.FromMinutes(-10));

        Assert.Equal(VisitDisplayStatus.InProgress, VisitStatusHelper.GetDisplayStatus(visit, Start.AddHours(2)));
    }
}
