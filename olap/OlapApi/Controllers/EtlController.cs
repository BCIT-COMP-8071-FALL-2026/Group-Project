using Microsoft.AspNetCore.Mvc;
using OlapApi.Data;
using OlapApi.Models;

namespace OlapApi.Controllers;

[ApiController]
[Route("api/etl")]
public class EtlController(IEtlRepository etl) : ControllerBase
{
    /// <summary>
    /// Full reload of the OlapDW star schema from OLTPAppDb (A8a + A8b, docs/olap/ETL-Mapping.md).
    /// Truncates and reloads every dimension and FactVisit in one atomic step; rerunnable.
    /// Returns the per-table row counts.
    /// </summary>
    [HttpPost("load")]
    public async Task<ActionResult<StarLoadSummary>> LoadStar(CancellationToken cancellationToken) =>
        Ok(await etl.LoadStarAsync(cancellationToken));
}
