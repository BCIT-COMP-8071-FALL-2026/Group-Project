using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using OLTPApp.E2E.Tests.Pages;

namespace OLTPApp.E2E.Tests;

// OLTP-1 acceptance criteria (docs/User-Stories.md).
[Collection(OltpAppCollection.Name)]
public class LoginTests(OltpAppFixture app) : PageTest
{
    // ASP.NET Core Identity defaults: 5 failed attempts lock the account for 5 minutes.
    private const int MaxFailedAttempts = 5;

    private LoginPage Login => new(Page);

    public override BrowserNewContextOptions ContextOptions() => new() { BaseURL = app.BaseUrl };

    [Fact]
    public async Task ValidLogin_ShowsHomePageAsSignedInUser()
    {
        var user = await app.CreateUserAsync();

        await Login.GotoAsync();
        await Login.LoginAsync(user.Email, user.Password);

        await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = $"Hello {user.Email}!" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("wrong password")]
    [InlineData("unknown email")]
    public async Task InvalidLogin_ShowsErrorAndStaysSignedOut(string scenario)
    {
        var user = await app.CreateUserAsync();
        var (email, password) = scenario == "unknown email"
            ? ($"nobody-{Guid.NewGuid():N}@example.test", user.Password)
            : (user.Email, "Wrong-Password1!");

        await Login.GotoAsync();
        await Login.LoginAsync(email, password);

        await Expect(Login.ErrorSummary).ToContainTextAsync("Invalid login attempt.");
        await Expect(Page).ToHaveURLAsync(new Regex(Regex.Escape(LoginPage.Path)));
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task FiveFailedAttempts_LockAccount_EvenForCorrectPassword()
    {
        var user = await app.CreateUserAsync();
        await Login.GotoAsync();

        for (var attempt = 1; attempt < MaxFailedAttempts; attempt++)
        {
            await Login.LoginAsync(user.Email, "Wrong-Password1!");
            await Expect(Login.ErrorSummary).ToContainTextAsync("Invalid login attempt.");
        }

        await Login.LoginAsync(user.Email, "Wrong-Password1!");
        await ExpectLockoutPageAsync();

        var lockoutEnd = await app.GetLockoutEndAsync(user.Email);
        Assert.NotNull(lockoutEnd);
        Assert.InRange(lockoutEnd.Value - DateTimeOffset.UtcNow, TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(5));

        await Login.GotoAsync();
        await Login.LoginAsync(user.Email, user.Password);
        await ExpectLockoutPageAsync();
    }

    private async Task ExpectLockoutPageAsync()
    {
        await Expect(Page).ToHaveURLAsync(new Regex(Regex.Escape(LoginPage.LockoutPath)));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Locked out" })).ToBeVisibleAsync();
    }
}
