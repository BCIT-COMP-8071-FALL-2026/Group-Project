using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OLTPApp.Data;
using OLTPApp.Models;
using OLTPApp.E2E.Tests.Pages;

namespace OLTPApp.E2E.Tests;

public sealed record TestUser(string Email, string Password);

// Starts OLTPApp once for the whole collection (unless OLTP_E2E_BASE_URL points at a running one)
// and seeds throwaway accounts through Identity, so tests don't depend on the Register page.
public sealed class OltpAppFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);

    private readonly E2ESettings _settings = E2ESettings.FromEnvironment();
    private readonly StringBuilder _appOutput = new();
    private readonly List<string> _createdUsers = new();
    private Process? _app;
    private ServiceProvider? _services;

    public string BaseUrl { get; private set; } = "";

    public async Task InitializeAsync()
    {
        BaseUrl = _settings.BaseUrl ?? StartApp();
        await WaitUntilReadyAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(_settings.ConnectionString));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        _services = services.BuildServiceProvider();
    }

    // Each test gets its own account, so the lockout test can't affect the others and reruns start clean.
    public async Task<TestUser> CreateUserAsync()
    {
        var user = new TestUser($"e2e-{Guid.NewGuid():N}@example.test", $"Test-{Guid.NewGuid():N}A1!");
        await WithUserManagerAsync(async users =>
        {
            var result = await users.CreateAsync(
                new ApplicationUser { UserName = user.Email, Email = user.Email, EmailConfirmed = true },
                user.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not seed {user.Email}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        });
        _createdUsers.Add(user.Email);
        return user;
    }

    public Task<DateTimeOffset?> GetLockoutEndAsync(string email) =>
        WithUserManagerAsync(async users => await users.GetLockoutEndDateAsync(
            await users.FindByEmailAsync(email) ?? throw new InvalidOperationException($"{email} not found")));

    public async Task DisposeAsync()
    {
        if (_services is not null)
        {
            foreach (var email in _createdUsers)
            {
                await WithUserManagerAsync(async users =>
                {
                    if (await users.FindByEmailAsync(email) is { } user)
                    {
                        await users.DeleteAsync(user);
                    }
                });
            }
            await _services.DisposeAsync();
        }

        if (_app is { HasExited: false })
        {
            _app.Kill(entireProcessTree: true);
            await _app.WaitForExitAsync();
        }
        _app?.Dispose();
    }

    private async Task WithUserManagerAsync(Func<UserManager<ApplicationUser>, Task> action) =>
        await WithUserManagerAsync(async users => { await action(users); return true; });

    private async Task<T> WithUserManagerAsync<T>(Func<UserManager<ApplicationUser>, Task<T>> action)
    {
        await using var scope = _services!.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }

    private string StartApp()
    {
        var url = $"http://127.0.0.1:{GetFreePort()}";
        var start = new ProcessStartInfo("dotnet")
        {
            ArgumentList = { "run", "--project", _settings.AppProject, "--no-launch-profile" },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        start.Environment["ASPNETCORE_URLS"] = url;
        // Env vars beat appsettings and user secrets, so the app always uses the E2E database.
        start.Environment["ConnectionStrings__DefaultConnection"] = _settings.ConnectionString;

        _app = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet run.");
        _app.OutputDataReceived += (_, e) => { lock (_appOutput) _appOutput.AppendLine(e.Data); };
        _app.ErrorDataReceived += (_, e) => { lock (_appOutput) _appOutput.AppendLine(e.Data); };
        _app.BeginOutputReadLine();
        _app.BeginErrorReadLine();
        return url;
    }

    // The app migrates its database on startup, so the login page answering means seeding is safe.
    private async Task WaitUntilReadyAsync()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var deadline = DateTime.UtcNow + StartupTimeout;
        while (DateTime.UtcNow < deadline)
        {
            if (_app is { HasExited: true })
            {
                throw new InvalidOperationException($"OLTPApp exited during startup:\n{AppOutput()}");
            }
            try
            {
                if ((await http.GetAsync(BaseUrl + LoginPage.Path)).IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }
            await Task.Delay(500);
        }
        throw new TimeoutException($"{BaseUrl}{LoginPage.Path} did not respond within {StartupTimeout}.\n{AppOutput()}");
    }

    private string AppOutput()
    {
        lock (_appOutput) return _appOutput.ToString();
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}

[CollectionDefinition(Name)]
public sealed class OltpAppCollection : ICollectionFixture<OltpAppFixture>
{
    public const string Name = "OLTPApp";
}
