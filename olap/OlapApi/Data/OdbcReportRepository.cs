using System.Data;
using System.Data.Odbc;
using OlapApi.Models;

namespace OlapApi.Data;

/// <summary>
/// Calls the report stored procedures in the OLAP database through ODBC Driver 18 for SQL Server.
/// </summary>
public class OdbcReportRepository(string connectionString) : IReportRepository
{
    public async Task<IReadOnlyList<MissedVisitsMonth>> GetMissedVisitsByMonthAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new OdbcConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        // ODBC uses the CALL escape for procedures.
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "{CALL dbo.usp_MissedVisitsByMonth}";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var month = reader.GetOrdinal("Month");
        var visitCount = reader.GetOrdinal("VisitCount");
        var missedCount = reader.GetOrdinal("MissedCount");
        var missedPercent = reader.GetOrdinal("MissedPercent");

        var months = new List<MissedVisitsMonth>();
        while (await reader.ReadAsync(cancellationToken))
        {
            months.Add(new MissedVisitsMonth(
                reader.GetString(month),
                reader.GetInt32(visitCount),
                reader.GetInt32(missedCount),
                reader.GetDecimal(missedPercent)));
        }

        return months;
    }

    public async Task<IReadOnlyList<CaregiverLateness>> GetCaregiverLatenessAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new OdbcConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "{CALL dbo.usp_CaregiverLateness}";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var caregiverName = reader.GetOrdinal("CaregiverName");
        var avgMinutesLate = reader.GetOrdinal("AvgMinutesLate");

        var caregivers = new List<CaregiverLateness>();
        while (await reader.ReadAsync(cancellationToken))
        {
            caregivers.Add(new CaregiverLateness(
                reader.GetString(caregiverName),
                reader.GetDecimal(avgMinutesLate)));
        }

        return caregivers;
    }

    public async Task<IReadOnlyList<SeniorOutreach>> GetSeniorOutreachAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new OdbcConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "{CALL dbo.usp_SeniorOutreach}";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var seniorName = reader.GetOrdinal("SeniorName");
        var missedCount = reader.GetOrdinal("MissedCount");
        var serviceTypes = reader.GetOrdinal("ServiceTypes");
        var livesAlone = reader.GetOrdinal("LivesAlone");
        var ageBand = reader.GetOrdinal("AgeBand");

        var seniors = new List<SeniorOutreach>();
        while (await reader.ReadAsync(cancellationToken))
        {
            seniors.Add(new SeniorOutreach(
                reader.GetString(seniorName),
                reader.GetInt32(missedCount),
                reader.GetString(serviceTypes),
                reader.GetBoolean(livesAlone),
                reader.GetString(ageBand)));
        }

        return seniors;
    }
}
