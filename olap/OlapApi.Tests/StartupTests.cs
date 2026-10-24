using Microsoft.AspNetCore.Hosting;

namespace OlapApi.Tests;

public class StartupTests(OlapApiFactory factory) : IClassFixture<OlapApiFactory>
{
    [Fact]
    public void RefusesToStartWithoutAConnectionString()
    {
        // An environment variable that is set but empty (ConnectionStrings__DefaultConnection=)
        // arrives as "", not as a missing key.
        var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient(
            new FakeReportRepository(),
            builder => builder.UseSetting("ConnectionStrings:DefaultConnection", "")));

        Assert.Equal("Connection string 'DefaultConnection' not found.", error.Message);
    }
}
