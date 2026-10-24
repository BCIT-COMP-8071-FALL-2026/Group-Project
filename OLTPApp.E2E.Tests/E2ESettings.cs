namespace OLTPApp.E2E.Tests;

// All overrides are environment variables so CI and teammates can retarget the suite without code changes.
public sealed record E2ESettings(string? BaseUrl, string ConnectionString, string AppProject)
{
    // Separate database from the dev OLTPAppDb, so test accounts never mix with real data.
    private const string DefaultConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=OLTPAppDb_E2E;Trusted_Connection=True;MultipleActiveResultSets=true";

    public static E2ESettings FromEnvironment() => new(
        // Set to test an app that is already running (e.g. Mikko's branch). Leave unset and the fixture starts OLTPApp itself.
        BaseUrl: Environment.GetEnvironmentVariable("OLTP_E2E_BASE_URL")?.TrimEnd('/'),
        // Must be the same database the app under test uses, since test accounts are seeded straight into it.
        ConnectionString: Environment.GetEnvironmentVariable("OLTP_E2E_CONNECTION_STRING") ?? DefaultConnectionString,
        AppProject: Environment.GetEnvironmentVariable("OLTP_E2E_APP_PROJECT")
            ?? Path.Combine(FindRepoRoot(), "OLTPApp", "OLTPApp.csproj"));

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GroupProject.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new InvalidOperationException(
            "Could not find GroupProject.sln above the test output folder. Set OLTP_E2E_APP_PROJECT instead.");
    }
}
