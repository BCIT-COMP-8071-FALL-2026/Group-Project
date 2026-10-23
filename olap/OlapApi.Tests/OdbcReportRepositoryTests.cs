using System.Data.Odbc;
using OlapApi.Data;
using OlapApi.Models;

namespace OlapApi.Tests;

/// <summary>
/// Runs <see cref="OdbcReportRepository"/> over ODBC against a throwaway database on a real SQL Server.
/// The procedure there is a stand-in that returns the contract's worked example in the contract's column
/// types, so these tests check the ODBC call and the column mapping, not the Query 1 logic itself.
/// </summary>
public class OdbcReportRepositoryTests : IAsyncLifetime
{
    private const string StandInProcedure = """
        CREATE PROCEDURE dbo.usp_MissedVisitsByMonth
        AS
        BEGIN
            SET NOCOUNT ON;
            SELECT CAST([Month] AS char(7)) AS [Month],
                   CAST(VisitCount AS int) AS VisitCount,
                   CAST(MissedCount AS int) AS MissedCount,
                   CAST(MissedPercent AS decimal(5, 1)) AS MissedPercent
            FROM (VALUES ('2025-01', 4, 1, 25.0),
                         ('2025-02', 3, 2, 66.7)) AS Months ([Month], VisitCount, MissedCount, MissedPercent)
            ORDER BY [Month];
        END
        """;

    private readonly string _database = $"OlapApiTests_{Guid.NewGuid():N}";

    private static string ServerConnectionString => SqlServerFactAttribute.ConnectionString!;

    private string DatabaseConnectionString =>
        new OdbcConnectionStringBuilder(ServerConnectionString) { ["Database"] = _database }.ConnectionString;

    [SqlServerFact]
    public async Task ReadsEveryRowAndColumnOfTheContract()
    {
        var months = await new OdbcReportRepository(DatabaseConnectionString).GetMissedVisitsByMonthAsync();

        Assert.Equal([new MissedVisitsMonth("2025-01", 4, 1, 25.0m), new MissedVisitsMonth("2025-02", 3, 2, 66.7m)], months);
    }

    [SqlServerFact]
    public async Task ReturnsNoRowsWhenThereAreNoVisits()
    {
        await ExecuteAsync(DatabaseConnectionString, StandInProcedure.Replace("CREATE PROCEDURE", "ALTER PROCEDURE")
            .Replace("ORDER BY [Month];", "WHERE 1 = 0 ORDER BY [Month];"));

        var months = await new OdbcReportRepository(DatabaseConnectionString).GetMissedVisitsByMonthAsync();

        Assert.Empty(months);
    }

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(SqlServerFactAttribute.ConnectionString))
        {
            return;
        }

        await ExecuteAsync(ServerConnectionString, $"CREATE DATABASE [{_database}]");
        await ExecuteAsync(DatabaseConnectionString, StandInProcedure);
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(SqlServerFactAttribute.ConnectionString))
        {
            return;
        }

        await ExecuteAsync(
            ServerConnectionString,
            $"ALTER DATABASE [{_database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_database}]");
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new OdbcConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new OdbcCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
