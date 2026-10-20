using Microsoft.AspNetCore.Mvc;
using OlapApi.Data;
using OlapApi.Models;

namespace OlapApi.Controllers;

[ApiController]
[Route("api/senior-visits")]
public class SeniorVisitsController(ISeniorVisitRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SeniorVisit>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await repository.GetAllAsync(cancellationToken));
}
