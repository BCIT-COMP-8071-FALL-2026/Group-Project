namespace OlapApi.Models;

/// <summary>
/// One row of Query 1 (see docs/Query1-Contract.md).
/// </summary>
/// <param name="Month">The month as YYYY-MM, e.g. 2025-10.</param>
/// <param name="VisitCount">Visits in the month, excluding cancelled and not yet due.</param>
/// <param name="MissedCount">Visits in <paramref name="VisitCount"/> that were missed.</param>
/// <param name="MissedPercent">MissedCount as a percentage of VisitCount, rounded to 1 decimal place.</param>
public record MissedVisitsMonth(string Month, int VisitCount, int MissedCount, decimal MissedPercent);
