namespace OlapApi.Models;

/// <summary>
/// One row of the SeniorVisits table (see olap/odbc-console-sample/setup.sql).
/// </summary>
public record SeniorVisit(int Id, string SeniorName, int VisitCount);
