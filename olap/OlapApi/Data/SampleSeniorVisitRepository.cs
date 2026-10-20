using OlapApi.Models;

namespace OlapApi.Data;

/// <summary>
/// Hardcoded rows that mirror the SeniorVisits seed data, so the chart works without a database.
/// Swap this for an ODBC-backed implementation in Program.cs once SQL Server is available.
/// </summary>
public class SampleSeniorVisitRepository : ISeniorVisitRepository
{
    private static readonly IReadOnlyList<SeniorVisit> Visits =
    [
        new(1, "Mary", 5),
        new(2, "John", 8),
        new(3, "Sara", 3),
    ];

    public Task<IReadOnlyList<SeniorVisit>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Visits);
}
