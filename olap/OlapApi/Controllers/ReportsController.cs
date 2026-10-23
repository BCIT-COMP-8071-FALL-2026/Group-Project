using Microsoft.AspNetCore.Mvc;
using OlapApi.Data;
using OlapApi.Models;

namespace OlapApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IReportRepository reports) : ControllerBase
{
    /// <summary>
    /// Query 1: visits, missed visits and missed % per month (docs/Query1-Contract.md).
    /// Takes no parameters; a query string is ignored.
    /// </summary>
    [HttpGet("missed-visits-by-month")]
    public async Task<ActionResult<IReadOnlyList<MissedVisitsMonth>>> GetMissedVisitsByMonth(
        CancellationToken cancellationToken) =>
        Ok(await reports.GetMissedVisitsByMonthAsync(cancellationToken));
}
