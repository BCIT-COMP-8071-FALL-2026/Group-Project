namespace OlapApi.Models;

/// <summary>
/// Row counts returned by a full star reload (A8a + A8b, docs/olap/ETL-Mapping.md).
/// </summary>
public record StarLoadSummary(
    int DimDate,
    int DimSenior,
    int DimProfessional,
    int DimService,
    int DimOrganization,
    int FactVisit);
