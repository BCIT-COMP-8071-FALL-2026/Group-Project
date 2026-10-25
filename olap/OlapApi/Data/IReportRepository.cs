using OlapApi.Models;

namespace OlapApi.Data;

/// <summary>
/// Runs the OLAP report procedures.
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Query 1: visits and missed visits per month, oldest first (docs/Query1-Contract.md).
    /// </summary>
    Task<IReadOnlyList<MissedVisitsMonth>> GetMissedVisitsByMonthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Query 3: average minutes late per caregiver, most late first (docs/OLAP_Queries.md).
    /// </summary>
    Task<IReadOnlyList<CaregiverLateness>> GetCaregiverLatenessAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Query 2/4: seniors with 3+ missed visits in the 90 days before the anchor date, most missed first
    /// (docs/OLAP_Queries.md).
    /// </summary>
    Task<IReadOnlyList<SeniorOutreach>> GetSeniorOutreachAsync(CancellationToken cancellationToken = default);
}
