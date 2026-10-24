using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace OlapApi.Tests;

/// <summary>
/// The React client runs on a different origin than the API, so the browser needs CORS headers to read responses.
/// </summary>
public class CorsTests(OlapApiFactory factory) : IClassFixture<OlapApiFactory>
{
    private const string Url = "/api/reports/missed-visits-by-month";

    [Fact]
    public async Task AllowsTheConfiguredClientOrigin()
    {
        var response = await Get(factory.CreateClient(new FakeReportRepository()), OlapApiFactory.ClientOrigin);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([OlapApiFactory.ClientOrigin], response.Headers.GetValues("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task AnswersThePreflightRequestFromTheClientOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, Url);
        request.Headers.Add("Origin", OlapApiFactory.ClientOrigin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await factory.CreateClient(new FakeReportRepository()).SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal([OlapApiFactory.ClientOrigin], response.Headers.GetValues("Access-Control-Allow-Origin"));
        Assert.Equal(["GET"], response.Headers.GetValues("Access-Control-Allow-Methods"));
    }

    [Fact]
    public async Task DoesNotAllowOtherOrigins()
    {
        var response = await Get(factory.CreateClient(new FakeReportRepository()), "http://elsewhere.test");

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task IgnoresATrailingSlashOnAConfiguredOrigin()
    {
        var client = factory.CreateClient(
            new FakeReportRepository(),
            builder => builder.UseSetting("Cors:AllowedOrigins:0", "http://slash.test/"));

        var response = await Get(client, "http://slash.test");

        Assert.Equal(["http://slash.test"], response.Headers.GetValues("Access-Control-Allow-Origin"));
    }

    private static Task<HttpResponseMessage> Get(HttpClient client, string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, Url);
        request.Headers.Add("Origin", origin);
        return client.SendAsync(request);
    }
}
