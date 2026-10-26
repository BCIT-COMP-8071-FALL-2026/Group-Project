using System.Net;
using System.Text.Json;
using OlapApi.Models;

namespace OlapApi.Tests;

/// <summary>
/// Query 3 endpoint, held to the same status and error rules as docs/Query1-Contract.md.
/// </summary>
public class CaregiverLatenessEndpointTests(OlapApiFactory factory) : IClassFixture<OlapApiFactory>
{
    private const string Url = "/api/reports/caregiver-lateness";

    private static readonly CaregiverLateness[] Rows =
    [
        new("Ava Chen", 32.5m),
        new("Ben Ortiz", 12.0m),
    ];

    [Fact]
    public async Task ReturnsTheProcedureRowsAsJson()
    {
        var client = factory.CreateClient(new FakeReportRepository(caregivers: Rows));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        // Field names follow the procedure's output columns (olap/sql/procs/CaregiverLateness.sql).
        Assert.Equal(
            """[{"caregiverName":"Ava Chen","avgMinutesLate":32.5},"""
            + """{"caregiverName":"Ben Ortiz","avgMinutesLate":12.0}]""",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReturnsAnEmptyArrayWhenNoCaregiverIsLate()
    {
        var client = factory.CreateClient(new FakeReportRepository(caregivers: []));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task IgnoresAQueryString()
    {
        var client = factory.CreateClient(new FakeReportRepository(caregivers: Rows));

        var plain = await client.GetStringAsync(Url);
        var response = await client.GetAsync($"{Url}?caregiver=1&foo=bar");

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
        var client = factory.CreateClient(new FakeReportRepository(caregivers: Rows));

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), Url));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
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
        Assert.DoesNotMatch(@"(?i)at OlapApi\.|StackTrace|Server=|Password=|SqlException|OdbcException|ODBC Driver", body);
    }
}
