using OlapApi.Models;

namespace OlapApi.Data;

/// <summary>
/// Runs the ETL that reloads the OLAP star schema from OLTPAppDb.
/// </summary>
public interface IEtlRepository
{
    /// <summary>
    /// Full reload of all dimensions and FactVisit from OLTPAppDb (A8a + A8b,
    /// docs/olap/ETL-Mapping.md). Rerunnable: the star ends up holding OLTPAppDb
    /// data only. Returns the per-table row counts.
    /// </summary>
    Task<StarLoadSummary> LoadStarAsync(CancellationToken cancellationToken = default);
}
