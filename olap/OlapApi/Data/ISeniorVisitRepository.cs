using OlapApi.Models;

namespace OlapApi.Data;

public interface ISeniorVisitRepository
{
    Task<IReadOnlyList<SeniorVisit>> GetAllAsync(CancellationToken cancellationToken = default);
}
