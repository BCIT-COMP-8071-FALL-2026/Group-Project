using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OlapApi.Data;

namespace OlapApi.Tests;

/// <summary>
/// Hosts OlapApi in memory with test configuration. Each client gets its own fake report repository.
/// </summary>
public class OlapApiFactory : WebApplicationFactory<Program>
{
    public const string ClientOrigin = "http://client.test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development, so appsettings.Development.json stays out of the tests.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Driver={unused in tests}");
        builder.UseSetting("Cors:AllowedOrigins:0", ClientOrigin);
    }

    public HttpClient CreateClient(IReportRepository reports, Action<IWebHostBuilder>? configure = null) =>
        WithWebHostBuilder(builder =>
        {
            configure?.Invoke(builder);
            builder.ConfigureTestServices(services => services.AddSingleton(reports));
        }).CreateClient();
}
