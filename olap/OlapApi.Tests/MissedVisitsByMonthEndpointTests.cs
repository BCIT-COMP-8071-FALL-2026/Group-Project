using System.Net;
using System.Text.Json;
using OlapApi.Models;

namespace OlapApi.Tests;

/// <summary>
/// The endpoint half of docs/Query1-Contract.md, including the checks in Leon's Postman collection.
/// </summary>
public class MissedVisitsByMonthEndpointTests(OlapApiFactory factory) : IClassFixture<OlapApiFactory>
{
    private const string Url = "/api/reports/missed-visits-by-month";

    // The procedure output from the contract's worked example.
    private static readonly MissedVisitsMonth[] WorkedExample =
    [
        new("2025-01", 4, 1, 25.0m),
        new("2025-02", 3, 2, 66.7m),
    ];

    [Fact]
    public async Task ReturnsTheWorkedExampleAsTheContractJson()
    {
        var client = factory.CreateClient(new FakeReportRepository(WorkedExample));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        // The React client and Postman depend on these exact names and formats.
        Assert.Equal(
            """[{"month":"2025-01","visitCount":4,"missedCount":1,"missedPercent":25.0},"""
            + """{"month":"2025-02","visitCount":3,"missedCount":2,"missedPercent":66.7}]""",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReturnsAnEmptyArrayWhenThereAreNoVisits()
    {
        var client = factory.CreateClient(new FakeReportRepository([]));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task IgnoresAQueryString()
    {
        var client = factory.CreateClient(new FakeReportRepository(WorkedExample));

        var plain = await client.GetStringAsync(Url);
        var response = await client.GetAsync($"{Url}?agency=1&foo=bar&from=not-a-month");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(plain, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task RejectsOtherMethods(string method)
    {
        var client = factory.CreateClient(new FakeReportRepository(WorkedExample));

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), Url));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task ReturnsNotFoundForAMistypedPath()
    {
        var client = factory.CreateClient(new FakeReportRepository(WorkedExample));

        var response = await client.GetAsync("/api/reports/missed-visits-by-mnth");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReturnsA500WithoutDatabaseDetailsWhenTheQueryFails()
    {
        var client = factory.CreateClient(new FakeReportRepository(failure: new InvalidOperationException(
            "ERROR [08001] [Microsoft][ODBC Driver 18 for SQL Server]Login timeout. Server=sql;Password=secret")));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(500, JsonDocument.Parse(body).RootElement.GetProperty("status").GetInt32());
        // The same check as the "Error does not leak internals" Postman test; details stay in the server log.
        Assert.DoesNotMatch(@"(?i)at OlapApi\.|StackTrace|Server=|Password=|SqlException|OdbcException|ODBC Driver", body);
    }
}
