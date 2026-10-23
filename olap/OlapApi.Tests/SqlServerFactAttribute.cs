namespace OlapApi.Tests;

/// <summary>
/// A test that needs a real SQL Server and ODBC Driver 18. It is skipped unless
/// OLAP_TEST_SQLSERVER holds an ODBC connection string to a server the tests can create databases on.
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public const string ConnectionStringVariable = "OLAP_TEST_SQLSERVER";

    public static string? ConnectionString => Environment.GetEnvironmentVariable(ConnectionStringVariable);

    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            Skip = $"Set {ConnectionStringVariable} to an ODBC connection string to run the SQL Server tests.";
        }
    }
}
