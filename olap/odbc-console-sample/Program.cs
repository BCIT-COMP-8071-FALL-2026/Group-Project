using System.Data.Odbc;

var connectionString =
    "Driver={ODBC Driver 18 for SQL Server};" +
    "Server=localhost;" +
    "Database=SeniorCareTest;" +
    "Trusted_Connection=yes;" +
    "TrustServerCertificate=yes;";

using var connection = new OdbcConnection(connectionString);

try
{
    connection.Open();

    Console.WriteLine("Connected to SQL Server successfully!");
    Console.WriteLine();

    var sql = "SELECT Id, SeniorName, VisitCount FROM SeniorVisits";

    using var command = new OdbcCommand(sql, connection);
    using var reader = command.ExecuteReader();

    Console.WriteLine("Senior Visits:");
    Console.WriteLine("----------------");

    var hasRows = false;

    while (reader.Read())
    {
        hasRows = true;

        var id = reader.GetInt32(0);
        var seniorName = reader.GetString(1);
        var visitCount = reader.GetInt32(2);

        Console.WriteLine($"{id} | {seniorName} | {visitCount}");
    }

    if (!hasRows)
    {
        Console.WriteLine("No rows found.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Database connection failed:");
    Console.WriteLine(ex.Message);
}