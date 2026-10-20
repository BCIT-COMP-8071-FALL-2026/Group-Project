# OLTPApp.E2E.Tests

Browser tests for OLTPApp, using Playwright + xUnit. The first spike covers OLTP-1 login: valid login, invalid login (wrong password and unknown email), and lockout after 5 failed attempts.

## How it works

- `OltpAppFixture` starts OLTPApp once with `dotnet run` on a free port, pointed at a separate **`OLTPAppDb_E2E`** database, so your dev `OLTPAppDb` is never touched. The app creates and migrates that database on startup.
- Each test seeds its own throwaway account through Identity's `UserManager`, so tests don't depend on the Register page, and a locked account can't affect other tests. The accounts are deleted when the run ends.
- All login selectors are in `Pages/LoginPage.cs`. The tests only use that page object.

## Running locally

Prerequisites: the same as OLTPApp (.NET 9, LocalDB). From the repo root:

```bash
dotnet build OLTPApp.E2E.Tests
```

Install the Playwright browser (first time only, and after a Playwright package upgrade). Without PowerShell 7, use `powershell -ExecutionPolicy Bypass -File` instead of `pwsh`:

```bash
pwsh OLTPApp.E2E.Tests/bin/Debug/net9.0/playwright.ps1 install chromium
```

Run the tests:

```bash
dotnet test OLTPApp.E2E.Tests
```

If you only have the .NET 10 runtime (no .NET 9), set `DOTNET_ROLL_FORWARD=Major` before building and running. To watch the browser, set `HEADED=1`.

## Configuration (environment variables)

| Variable | Default | Use |
|---|---|---|
| `OLTP_E2E_BASE_URL` | unset, so the fixture starts OLTPApp | Test an app that's already running, e.g. `http://localhost:5163` |
| `OLTP_E2E_CONNECTION_STRING` | LocalDB, `OLTPAppDb_E2E` | The database to seed test accounts into. It **must** be the one the app under test uses. In CI, point it at the SQL Server service container |
| `OLTP_E2E_APP_PROJECT` | `<repo>/OLTPApp/OLTPApp.csproj` | Start a different checkout of the app |

## Switching to Mikko's login

When the new login page lands:

1. Merge or check out that branch. The fixture starts whatever `OLTPApp` is in the checkout. Or set `OLTP_E2E_APP_PROJECT` to point at another worktree.
2. Update the selectors, `Path` and `LockoutPath` in `Pages/LoginPage.cs`, and the expected messages in `LoginTests.cs` if the wording changes.
3. Swap the stock IDs (`Input_Email`, `Input_Password`, `login-submit`) for the stable IDs agreed with Mikko.

## Known result on `develop`

`FiveFailedAttempts_LockAccount_EvenForCorrectPassword` **fails**. `Login.cshtml.cs` calls `PasswordSignInAsync(..., lockoutOnFailure: false)`, so failed attempts are never counted. The 5th wrong password shows "Invalid login attempt." again instead of the Lockout page. OLTP-1 requires the account to lock after 5 failures. This is an app bug for the devs, not a test problem.
