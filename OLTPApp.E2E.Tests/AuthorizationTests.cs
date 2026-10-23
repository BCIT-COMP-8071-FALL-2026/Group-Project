using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using OLTPApp.Authorization;
using OLTPApp.E2E.Tests.Pages;

namespace OLTPApp.E2E.Tests;

// #57 / docs/Roles-and-Permissions.md: every page needs login except Login and Register.
// Per-role 403 checks land with each role's page (#61-#66), since no role-protected action exists yet.
[Collection(OltpAppCollection.Name)]
public class AuthorizationTests(OltpAppFixture app) : PageTest
{
    private const string WaitingForRoleMessage = "Waiting for Admin to assign a role";

    private LoginPage Login => new(Page);

    public override BrowserNewContextOptions ContextOptions() => new() { BaseURL = app.BaseUrl };

    [Fact]
    public async Task AnonymousUser_IsRedirectedToLogin()
    {
        await Page.GotoAsync("/");

        await Expect(Page).ToHaveURLAsync(new Regex(Regex.Escape(LoginPage.Path)));
    }

    [Fact]
    public async Task AnonymousUser_CanLoadStaticAssets()
    {
        var response = await Page.APIRequest.GetAsync("/css/site.css");

        Assert.Equal(200, response.Status);
    }

    [Fact]
    public async Task UserWithNoRole_SeesWaitingForAdminMessage()
    {
        var user = await app.CreateUserAsync();

        await Login.GotoAsync();
        await Login.LoginAsync(user.Email, user.Password);

        await Expect(Page.GetByText(WaitingForRoleMessage)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UserWithRole_DoesNotSeeWaitingForAdminMessage()
    {
        var user = await app.CreateUserAsync(RoleNames.Caregiver);

        await Login.GotoAsync();
        await Login.LoginAsync(user.Email, user.Password);

        await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/");
        // Wait for the signed-in page to render, so the count check can't pass on a blank page
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = $"Hello {user.Email}!" })).ToBeVisibleAsync();
        await Expect(Page.GetByText(WaitingForRoleMessage)).ToHaveCountAsync(0);
    }
}
