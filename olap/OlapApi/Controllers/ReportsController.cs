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

    /// <summary>
    /// Query 3: average minutes late per caregiver, over Late visits only (docs/OLAP_Queries.md).
    /// Takes no parameters; a query string is ignored.
    /// </summary>
    [HttpGet("caregiver-lateness")]
    public async Task<ActionResult<IReadOnlyList<CaregiverLateness>>> GetCaregiverLateness(
        CancellationToken cancellationToken) =>
        Ok(await reports.GetCaregiverLatenessAsync(cancellationToken));

    /// <summary>
    /// Query 2/4: seniors with 3+ missed visits in the 90 days before the anchor date (docs/OLAP_Queries.md).
    /// Takes no parameters; a query string is ignored.
    /// </summary>
    [HttpGet("senior-outreach")]
    public async Task<ActionResult<IReadOnlyList<SeniorOutreach>>> GetSeniorOutreach(
        CancellationToken cancellationToken) =>
        Ok(await reports.GetSeniorOutreachAsync(cancellationToken));
}
