using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OlapApi.Models;

namespace OlapApi.Tests;

public class SeniorVisitsEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetSeniorVisits_ReturnsSampleRows()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/senior-visits");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var visits = await response.Content.ReadFromJsonAsync<List<SeniorVisit>>();
        Assert.Equal(
            [new SeniorVisit(1, "Mary", 5), new SeniorVisit(2, "John", 8), new SeniorVisit(3, "Sara", 3)],
            visits);
    }

    [Fact]
    public async Task GetSeniorVisits_UsesCamelCasePropertyNames()
    {
        // The React client reads seniorName and visitCount, so the JSON contract must stay camelCase.
        var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/senior-visits"));

        var first = json.RootElement[0];
        Assert.Equal(1, first.GetProperty("id").GetInt32());
        Assert.Equal("Mary", first.GetProperty("seniorName").GetString());
        Assert.Equal(5, first.GetProperty("visitCount").GetInt32());
    }
}
