using OLTPApp.Models;

namespace OLTPApp.Services;

// Display-only status (OLTP-17). Missed is computed here and never stored, so VisitStatus has no Missed
public enum VisitDisplayStatus { Scheduled, InProgress, Completed, Cancelled, Missed }

// The one Missed rule for visit pages (#62, #63, #65); matches Missed in docs/Query1-Contract.md
public static class VisitStatusHelper
{
    public static readonly TimeSpan MissedCutoff = TimeSpan.FromMinutes(60);

    public static VisitDisplayStatus GetDisplayStatus(Visit visit) => GetDisplayStatus(visit, DateTime.UtcNow);

    // All times UTC with exact comparison: a check-in at +60:00 is within, +60:00.4 is Missed.
    // A check-in after the cutoff stays Missed even when Completed.
    public static VisitDisplayStatus GetDisplayStatus(Visit visit, DateTime utcNow)
    {
        if (visit.Status == VisitStatus.Cancelled) return VisitDisplayStatus.Cancelled;

        var cutoff = visit.ScheduledStart + MissedCutoff;
        var isMissed = cutoff <= utcNow && (visit.ActualCheckIn is not { } checkIn || checkIn > cutoff);
        if (isMissed) return VisitDisplayStatus.Missed;

        return visit.Status switch
        {
            VisitStatus.InProgress => VisitDisplayStatus.InProgress,
            VisitStatus.Completed => VisitDisplayStatus.Completed,
            _ => VisitDisplayStatus.Scheduled,
        };
    }
}
