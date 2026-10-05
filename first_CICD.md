# GitHub Actions CI/CD Setup

## Project: COMP 8071 Group Project
Two apps, two workflows.

---

## OLTP (ASP.NET Core MVC + EF)

```yaml
# .github/workflows/oltp-ci.yml
name: OLTP CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0"
      - name: Scaffold sample app (remove once Assignment 1b code lands)
        run: dotnet new mvc -n OltpApp --no-restore
      - run: dotnet restore OltpApp/OltpApp.csproj
      - run: dotnet build OltpApp/OltpApp.csproj --no-restore
      - name: Tests placeholder (no test project yet)
        run: echo "dotnet test will run once a test project is added"
```

---

## OLAP (ASP.NET Web API + React)

```yaml
# .github/workflows/olap-ci.yml
name: OLAP CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  api:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0"
      - name: Scaffold sample API (remove once Assignment 1d code lands)
        run: dotnet new webapi -n OlapApi --no-restore
      - run: dotnet restore OlapApi/OlapApi.csproj
      - run: dotnet build OlapApi/OlapApi.csproj --no-restore
      - name: Tests placeholder (no test project yet)
        run: echo "dotnet test will run once a test project is added"

  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: "20"
      - run: npm ci
        working-directory: ./frontend  # adjust to your React folder
      - run: npm run build
        working-directory: ./frontend
```

---

## How to add these

```bash
mkdir -p .github/workflows
# create the yml files above
git add .github/
git commit -m "ci: add GitHub Actions for OLTP and OLAP"
git push
```

Once pushed: repo → **Actions** tab to see runs.

**Note:** SQL Server tests won't run in CI without a service container. Skip DB tests for now or add a `sql-server` service block later.
