using System.Net;
using System.Text.Json;
using OlapApi.Models;

namespace OlapApi.Tests;

/// <summary>
/// Query 2/4 endpoint, held to the same status and error rules as docs/Query1-Contract.md.
/// </summary>
public class SeniorOutreachEndpointTests(OlapApiFactory factory) : IClassFixture<OlapApiFactory>
{
    private const string Url = "/api/reports/senior-outreach";

    private static readonly SeniorOutreach[] Rows =
    [
        new("Grace Lee", 5, "Meal Prep, Personal Care", true, "85+"),
        new("Henry Park", 3, "Companionship", false, "65-74"),
    ];

    [Fact]
    public async Task ReturnsTheProcedureRowsAsJson()
    {
        var client = factory.CreateClient(new FakeReportRepository(seniors: Rows));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        // Field names follow the procedure's output columns (olap/sql/procs/SeniorOutreach.sql).
        Assert.Equal(
            """[{"seniorName":"Grace Lee","missedCount":5,"serviceTypes":"Meal Prep, Personal Care","livesAlone":true,"ageBand":"85+"},"""
            + """{"seniorName":"Henry Park","missedCount":3,"serviceTypes":"Companionship","livesAlone":false,"ageBand":"65-74"}]""",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReturnsAnEmptyArrayWhenNoSeniorQualifies()
    {
        var client = factory.CreateClient(new FakeReportRepository(seniors: []));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task IgnoresAQueryString()
    {
        var client = factory.CreateClient(new FakeReportRepository(seniors: Rows));

        var plain = await client.GetStringAsync(Url);
        var response = await client.GetAsync($"{Url}?days=30&foo=bar");

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
        var client = factory.CreateClient(new FakeReportRepository(seniors: Rows));

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
