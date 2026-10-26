using OlapApi.Data;
using OlapApi.Models;

namespace OlapApi.Tests;

/// <summary>
/// Returns fixed rows (or throws) instead of calling the database.
/// </summary>
public class FakeReportRepository(
    IReadOnlyList<MissedVisitsMonth>? months = null,
    Exception? failure = null,
    IReadOnlyList<CaregiverLateness>? caregivers = null,
    IReadOnlyList<SeniorOutreach>? seniors = null)
    : IReportRepository
{
    public Task<IReadOnlyList<MissedVisitsMonth>> GetMissedVisitsByMonthAsync(CancellationToken cancellationToken = default) =>
        Result(months);

    public Task<IReadOnlyList<CaregiverLateness>> GetCaregiverLatenessAsync(CancellationToken cancellationToken = default) =>
        Result(caregivers);

    public Task<IReadOnlyList<SeniorOutreach>> GetSeniorOutreachAsync(CancellationToken cancellationToken = default) =>
        Result(seniors);

    private Task<IReadOnlyList<T>> Result<T>(IReadOnlyList<T>? rows) =>
        failure is null
            ? Task.FromResult(rows ?? [])
            : Task.FromException<IReadOnlyList<T>>(failure);
}
