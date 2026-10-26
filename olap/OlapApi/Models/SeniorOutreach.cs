namespace OlapApi.Models;

/// <summary>
/// One row of Query 2/4 (see docs/OLAP_Queries.md and olap/sql/procs/SeniorOutreach.sql).
/// </summary>
/// <param name="SeniorName">The senior's first and last name.</param>
/// <param name="MissedCount">Missed visits in the 90 days before the anchor date; always 3 or more.</param>
/// <param name="ServiceTypes">Service names of the missed visits, comma-separated and sorted.</param>
/// <param name="LivesAlone">Whether the senior lives alone.</param>
/// <param name="AgeBand">Under 65, 65-74, 75-84 or 85+, at the anchor date.</param>
public record SeniorOutreach(string SeniorName, int MissedCount, string ServiceTypes, bool LivesAlone, string AgeBand);
