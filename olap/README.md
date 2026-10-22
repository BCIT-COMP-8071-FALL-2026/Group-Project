# OLAP app

The OLAP app shows Query 1 (missed visits per month) as a Chart.js line chart and a table, with a print button.
The React client calls an ASP.NET Core Web API, and the API runs the Query 1 stored procedure over ODBC.
The procedure and the JSON it becomes are defined in the [Query 1 contract](../docs/Query1-Contract.md).

## Layout

| Path | What it is |
|---|---|
| `OlapApi/` | ASP.NET Core Web API (.NET 9) exposing `GET /api/reports/missed-visits-by-month` |
| `OlapApi.Tests/` | xUnit tests for the JSON contract, CORS and the ODBC call |
| `olap-client/` | React + TypeScript + Vite client with the chart, the table and printing |
| `Olap.slnx` | Solution containing the two .NET projects |

## Prerequisites

- .NET 9 SDK
- Node.js 22.13+ or 24+ (LTS) and npm
- A SQL Server OLAP database that has `dbo.usp_MissedVisitsByMonth`
- [ODBC Driver 18 for SQL Server](https://learn.microsoft.com/sql/connect/odbc/download-odbc-driver-for-sql-server) to run the API (Microsoft's page covers Windows, macOS and Linux)

## Configuration

Connection strings and URLs come from configuration, never from the code.

| Setting | Read by | Development value | Meaning |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | API | Local SQL Server, Windows login, database `OlapDW` (`OlapApi/appsettings.Development.json`) | ODBC connection string for the OLAP database. The API refuses to start without it. |
| `Cors:AllowedOrigins` | API | `http://localhost:5173` (Vite) and `http://localhost:3000` (Docker Compose) | Origins the client is served from. The browser only lets the client read API responses from these origins. |
| `VITE_API_BASE_URL` | Client, at build time | `http://localhost:5298` (`olap-client/.env.development`) | Where the browser reaches the API. Docker passes it as a build argument. If it is unset, the client calls `/api` on its own origin. |

In environment variables, `:` becomes `__`, for example `ConnectionStrings__DefaultConnection` and `Cors__AllowedOrigins__0`.
The connection string uses ODBC syntax, so it starts with `Driver={ODBC Driver 18 for SQL Server};` and uses `Uid`/`Pwd` rather than `User Id`/`Password`.

To use a different database locally, for example with a SQL login, put the connection string in user secrets so it never gets committed:

```bash
cd olap/OlapApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Driver={ODBC Driver 18 for SQL Server};Server=localhost,1433;Database=OlapDW;Uid=sa;Pwd=<password>;TrustServerCertificate=yes;"
```

The `OlapDW` database comes from the scripts in `sql/`: run `sql/ddl/00_create_database.sql`, `01_dimensions.sql` and `02_fact_visit.sql`, then the Query 1 procedure in `sql/procs/`.

## Run it locally

Start the API in one terminal:

```bash
cd olap
dotnet run --project OlapApi --launch-profile http
```

It listens on `http://localhost:5298`.
Check it with `curl http://localhost:5298/api/reports/missed-visits-by-month`.

Start the client in a second terminal:

```bash
cd olap/olap-client
npm install
npm run dev
```

Open `http://localhost:5173`.
The dev server always uses port 5173, because that is the origin the API allows in development.

## Printing

The Print button (or Ctrl+P / Cmd+P) prints only the report: the navigation and the button are hidden, and the chart is redrawn to fit the page.
The seed data's 15 months fit on one Letter portrait page (OLAP-4), and table rows are never split across pages.

## Test and lint

```bash
cd olap
dotnet test Olap.slnx

cd olap-client
npm test
npm run lint
npm run build
```

If you only have the .NET 10 runtime, run the .NET tests as `DOTNET_ROLL_FORWARD=Major dotnet test Olap.slnx`.

The two `OdbcReportRepositoryTests` run the real ODBC code against SQL Server, so `dotnet test` skips them unless `OLAP_TEST_SQLSERVER` is set.
Set it to an ODBC connection string for a server where the tests may create a throwaway database (they drop it afterwards):

```bash
OLAP_TEST_SQLSERVER='Driver={ODBC Driver 18 for SQL Server};Server=localhost,1433;Database=master;Uid=sa;Pwd=<password>;TrustServerCertificate=yes;' dotnet test Olap.slnx
```

In PowerShell, run `$env:OLAP_TEST_SQLSERVER = '...'` first.

## API

`GET /api/reports/missed-visits-by-month` takes no parameters and returns one object per month, oldest first:

```json
[
  { "month": "2025-01", "visitCount": 4, "missedCount": 1, "missedPercent": 25.0 },
  { "month": "2025-02", "visitCount": 3, "missedCount": 2, "missedPercent": 66.7 }
]
```

A query string is ignored, other methods get `405`, and a failed database call gets a `500` whose details stay in the API's log.
See the [Query 1 contract](../docs/Query1-Contract.md) for the full definition.
