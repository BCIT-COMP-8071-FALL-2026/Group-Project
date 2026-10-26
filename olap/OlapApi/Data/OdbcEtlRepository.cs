using System.Data;
using System.Data.Odbc;
using OlapApi.Models;

namespace OlapApi.Data;

/// <summary>
/// Runs the ETL stored procedure in the OLAP database through ODBC Driver 18 for SQL Server.
/// </summary>
public class OdbcEtlRepository(string connectionString) : IEtlRepository
{
    public async Task<StarLoadSummary> LoadStarAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new OdbcConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        // ODBC uses the CALL escape for procedures.
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "{CALL dbo.usp_LoadStar}";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("usp_LoadStar did not return a summary row.");
        }

        return new StarLoadSummary(
            reader.GetInt32(reader.GetOrdinal("DimDate")),
            reader.GetInt32(reader.GetOrdinal("DimSenior")),
            reader.GetInt32(reader.GetOrdinal("DimProfessional")),
            reader.GetInt32(reader.GetOrdinal("DimService")),
            reader.GetInt32(reader.GetOrdinal("DimOrganization")),
            reader.GetInt32(reader.GetOrdinal("FactVisit")));
    }
}
