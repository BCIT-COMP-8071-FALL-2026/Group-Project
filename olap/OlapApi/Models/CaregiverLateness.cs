namespace OlapApi.Models;

/// <summary>
/// One row of Query 3 (see docs/OLAP_Queries.md and olap/sql/procs/CaregiverLateness.sql).
/// </summary>
/// <param name="CaregiverName">The caregiver's first and last name.</param>
/// <param name="AvgMinutesLate">Average minutes late over the caregiver's Late visits, rounded to 1 decimal place.</param>
public record CaregiverLateness(string CaregiverName, decimal AvgMinutesLate);
