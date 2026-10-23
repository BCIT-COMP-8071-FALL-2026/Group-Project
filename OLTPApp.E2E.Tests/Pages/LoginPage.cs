using Microsoft.Playwright;

namespace OLTPApp.E2E.Tests.Pages;

// Every login selector lives here. When Mikko's login page lands, update this file, not the tests.
// The IDs below come from the scaffolded Identity page (asp-for="Input.Email" renders id="Input_Email").
public sealed class LoginPage(IPage page)
{
    public const string Path = "/Identity/Account/Login";
    public const string LockoutPath = "/Identity/Account/Lockout";

    public ILocator Email => page.Locator("#Input_Email");
    public ILocator Password => page.Locator("#Input_Password");
    public ILocator Submit => page.Locator("#login-submit");
    public ILocator ErrorSummary => page.Locator("form#account").GetByRole(AriaRole.Alert);

    public async Task GotoAsync() => await page.GotoAsync(Path);

    public async Task LoginAsync(string email, string password)
    {
        await Email.FillAsync(email);
        await Password.FillAsync(password);
        await Submit.ClickAsync();
    }
}
