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
}
