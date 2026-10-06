# OLAP sample

A React app renders a Chart.js bar chart from JSON served by an ASP.NET Core Web API.
The API currently returns hardcoded rows that mirror the `SeniorVisits` seed data from the ODBC sample.

## Layout

| Path | What it is |
|---|---|
| `OlapApi/` | ASP.NET Core Web API (.NET 10) exposing `GET /api/senior-visits` |
| `OlapApi.Tests/` | xUnit integration tests for the API (`WebApplicationFactory`) |
| `olap-client/` | React + TypeScript + Vite app with the Chart.js chart |
| `Olap.slnx` | Solution containing the two .NET projects |

## Prerequisites

- .NET 10 SDK
- Node.js 22.13+ or 24+ (LTS) and npm

## Run it

Start the API in one terminal:

```bash
cd olap
dotnet run --project OlapApi --launch-profile http
```

It listens on `http://localhost:5298`.
Check it with `curl http://localhost:5298/api/senior-visits`.

Start the React app in a second terminal:

```bash
cd olap/olap-client
npm install
npm run dev
```

Open the URL Vite prints (usually `http://localhost:5173`).
The dev server proxies `/api/*` to the API, so no CORS setup is needed.
If the API runs somewhere else, set `OLAP_API_URL`, for example `OLAP_API_URL=http://localhost:6000 npm run dev`.

## Test and lint

```bash
cd olap
dotnet test Olap.slnx

cd olap-client
npm test
npm run lint
npm run build
```

## API contract

`GET /api/senior-visits` returns:

```json
[
  { "id": 1, "seniorName": "Mary", "visitCount": 5 },
  { "id": 2, "seniorName": "John", "visitCount": 8 },
  { "id": 3, "seniorName": "Sara", "visitCount": 3 }
]
```

## Connecting the database

The controller reads from `ISeniorVisitRepository`.
`SampleSeniorVisitRepository` holds the hardcoded rows.
To read from SQL Server, add an ODBC-backed implementation of the interface (see `odbc-console-sample` on `feature/olap-odbc-sample`) and register it in `OlapApi/Program.cs` in place of the sample one.
The client and the JSON shape stay the same.
