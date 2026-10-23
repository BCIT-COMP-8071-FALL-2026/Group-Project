using OlapApi.Data;
using OlapApi.Models;

namespace OlapApi.Tests;

/// <summary>
/// Returns fixed rows (or throws) instead of calling the database.
/// </summary>
public class FakeReportRepository(IReadOnlyList<MissedVisitsMonth>? months = null, Exception? failure = null)
    : IReportRepository
{
    public Task<IReadOnlyList<MissedVisitsMonth>> GetMissedVisitsByMonthAsync(CancellationToken cancellationToken = default) =>
        failure is null
            ? Task.FromResult(months ?? [])
            : Task.FromException<IReadOnlyList<MissedVisitsMonth>>(failure);
}
