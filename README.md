# Group-Project

COMP 8071 group project. `OLTPApp` is an ASP.NET Core MVC app (.NET 9) with ASP.NET Core Identity for login, backed by SQL Server through Entity Framework Core. Migrations manage the database schema. Project docs (feature list, test plan, OLAP queries) are in `docs/`.

## Branching & PR Workflow

### The project has the "***main***" branch for the most stable release version of the code

### Current stable development work goes into the "***develop***" branch

### Any feature branches should be named with the convention "***feature/\****" with * indicating the feature name concisely

Any merges to ***main*** or ***develop*** must be done via a pull request. All pull requests must go through at least one manual approval from the qa team, and requests to the main branch must only be from the ***develop*** branch. Pull requests to the ***main*** branch must also be reviewed by 2 approvers, one on the QA team and one on the DevOps team. If any member of any team submits a *Request Change* request on the pr then it will be blocking until issues are resolved.

Last modified: 10/6/1026

## Local Setup (OLTPApp)

### Prerequisites

| Tool | Version | Check with |
|---|---|---|
| .NET SDK | 9.x (or 10.x with the .NET 9 runtime) | `dotnet --list-sdks` |
| SQL Server | LocalDB (recommended) **or** a full instance (Developer/Express) | `sqllocaldb info` / `Get-Service *SQL*` (PowerShell) |
| SSMS | Optional, for browsing the database | — |

### First-time setup

1. **Clone the repo and restore the pinned EF tool:**
   ```bash
   git clone https://github.com/BCIT-COMP-8071-FALL-2026/Group-Project.git
   cd Group-Project
   dotnet tool restore
   ```
   `dotnet tool restore` installs the `dotnet-ef` version pinned in `dotnet-tools.json` (9.0.20, matching the project's EF packages). Inside the repo it takes priority over any global `dotnet-ef` you have installed. Check it with `dotnet ef --version`.

2. **Point the app at your SQL Server (only if you don't use LocalDB).**
   `OLTPApp/appsettings.json` defaults to **LocalDB**: `Server=(localdb)\mssqllocaldb;Database=OLTPAppDb`. **If you have LocalDB, skip this step and move to step 3.**

   If you only have a full SQL Server instance, override the connection string with **user secrets**. They're stored in your Windows user profile, not in the repo, so your machine-specific setting never gets committed. Run this from the `OLTPApp` folder:
   ```bash
   cd OLTPApp
   dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'Server=<YOUR SERVER>;Database=OLTPAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true'
   ```

   | Your setup | `<YOUR SERVER>` |
   |---|---|
   | SQL Server installed as the default instance (service `MSSQLSERVER`) | `localhost` |
   | SQL Server Express named instance (service `MSSQL$SQLEXPRESS`) | `localhost\SQLEXPRESS` |

   For example, with a default instance:
   ```bash
   dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'Server=localhost;Database=OLTPAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true'
   ```
   It should print `Successfully saved ConnectionStrings:DefaultConnection to the secret store.` You can check the saved value with `dotnet user-secrets list`.

   Keep the single quotes. They stop Git Bash and PowerShell from mangling the backslash.

3. **Verify the connection target** (from the `OLTPApp` folder):
   ```bash
   dotnet ef dbcontext info
   ```
   You should see `Type: OLTPApp.Data.ApplicationDbContext`, `Database name: OLTPAppDb`, and a `Data source:` line showing `(localdb)\mssqllocaldb` or your server from the table above.

### Running the app

From the `OLTPApp` folder:
```bash
dotnet run
```

On startup the app calls `Database.Migrate()`, which creates the database if it doesn't exist and applies any pending migrations. You don't need to run `dotnet ef database update` yourself.

- **First run:** the log shows `CREATE DATABASE [OLTPAppDb]`, then `Applying migration '00000000000000_CreateIdentitySchema'`, followed by `CREATE TABLE` statements for the 7 Identity tables (`AspNetUsers`, `AspNetRoles`, ...) plus `__EFMigrationsHistory`.
- **Later runs:** the log shows `No migrations were applied. The database is already up to date.`

Open the URL printed after `Now listening on:` (e.g. `http://localhost:5163`).

### Registering and logging in

There's no email sender yet, so accounts are confirmed manually:
1. Click **Register** and create an account (the password needs an uppercase letter, a digit and a symbol, e.g. `Test123!`).
2. On the next page, click **Click here to confirm your account**. Don't skip it, or login will fail.
3. Click **Login** with the same email and password.

### Changing the database schema (migrations)

The EF context is `OLTPApp/Data/ApplicationDbContext.cs`, and migrations live in `OLTPApp/Data/Migrations/`.

After changing a model or `ApplicationDbContext`, run this from the `OLTPApp` folder:
```bash
dotnet ef migrations add <DescriptiveName> --output-dir Data/Migrations   # e.g. AddSeniorTable
dotnet ef database update                                                 # optional, the app also applies it on startup
```

Team rules:
- **Review the generated migration** before committing. Check that it does what you expect.
- **Commit the whole `Data/Migrations/` folder**, including `ApplicationDbContextModelSnapshot.cs`, in the same PR as the model change.
- **Never edit or delete a migration that has already been merged.** Add a new migration to fix it instead.
- **Teammates just pull and run.** Their database updates automatically.
- **If two PRs both add migrations,** the second one to merge must be rebased and have its migration regenerated, so the snapshot stays consistent.

### Troubleshooting

| Problem | Cause / Fix |
|---|---|
| `Invalid login attempt.` right after registering | The account isn't confirmed. Click the confirm link after **Register**, or run `UPDATE AspNetUsers SET EmailConfirmed = 1;` against `OLTPAppDb` in SSMS. |
| `There is already an object named '...' in the database` | Your `OLTPAppDb` was created some other way (e.g. by `EnsureCreated()` or an older version of the app), so it has no matching `__EFMigrationsHistory`. In SSMS, right-click `OLTPAppDb` → **Delete**, tick **Close existing connections**, then run the app again. **This deletes all data in that database.** |
| `dotnet ef`: `Could not execute because the specified command or file was not found` | The EF tool isn't installed. Run `dotnet tool restore` from inside the repo. |
| `dotnet ef` warns the tools version is newer than the runtime | You're using a global `dotnet-ef`. Run `dotnet tool restore` from inside the repo. |
| Your user-secrets connection string is ignored (app still uses LocalDB) | User secrets only load in the **Development** environment. `dotnet run` uses Development via `Properties/launchSettings.json`. Check that the log says `Hosting environment: Development` and that `ASPNETCORE_ENVIRONMENT` isn't set to something else. |
| `A network-related or instance-specific error occurred` | Wrong server name, or SQL Server isn't running. Re-check step 2, run `sqllocaldb info` for LocalDB, or confirm the service is `Running` with `Get-Service *SQL*`. |
| `The certificate chain was issued by an authority that is not trusted` | Make sure the connection string contains `TrustServerCertificate=True`. In SSMS, tick **Trust server certificate**. |
| SSMS (v21+) only offers `localhost` under Server name | The box is editable: click into it and type the server name, e.g. `(localdb)\MSSQLLocalDB`. |
| `warn: No store type was specified for the decimal property '...'` | A `decimal` property has no precision set, so SQL Server defaults to `decimal(18,2)` and silently rounds extra digits. In `ApplicationDbContext`, override `OnModelCreating`, call `base.OnModelCreating(modelBuilder)` first (Identity needs it), then set it explicitly, e.g. `modelBuilder.Entity<Visit>().Property(v => v.Cost).HasPrecision(18, 2);`, and add a migration. |

