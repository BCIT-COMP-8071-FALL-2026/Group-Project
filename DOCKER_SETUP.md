# Docker Setup Steps

Run the following commands from the folder containing `docker-compose.yml`.

### 0. Create the `.env` file
Compose reads two settings from a `.env` file next to `docker-compose.yml`. Copy the example and set your own password:
```powershell
Copy-Item .env.example .env
```

| Variable | Used by | Value |
|---|---|---|
| `SA_PASSWORD` | SQL Server, OLTP app, OLAP API | Password for the `sa` login. Must meet SQL Server's complexity rules. |
| `SQL_DATABASE` | OLAP API | `OlapDW` |

`.env` is gitignored, so never commit it. Compose stops with `Set SA_PASSWORD in .env` if the password is missing.

### 1. Build Docker Images
```powershell
docker compose build
```

### 2. Start All Containers
If this is your first time running the project, start SQL Server and run these scripts to set up the OLAP database. Make sure `.env` has `SQL_DATABASE=OlapDW`.

Only run this setup on a new report database. Running it again will recreate the tables and delete existing report data. The OLTP app sets up its own tables through migrations.

```powershell
docker compose up -d --wait sqlserver
$sqlFiles = @(
    'olap/sql/ddl/00_create_database.sql',
    'olap/sql/ddl/01_dimensions.sql',
    'olap/sql/ddl/02_fact_visit.sql',
    'olap/sql/procs/MissedVisitsByMonth.sql'
)
foreach ($sqlFile in $sqlFiles) {
    Get-Content -Raw -LiteralPath $sqlFile -ErrorAction Stop | docker compose exec -T sqlserver bash -c 'SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -b'
    if ($LASTEXITCODE -ne 0) { throw "SQL initialization failed: $sqlFile" }
}
```

The report will be empty until visit data is added.

Once that's done, start everything with the command below. Next time you run the project, skip the database setup and just use this command.
```powershell
docker compose up --build
```

### 3. Check Running Containers
```powershell
docker compose ps
```

### 4. Application URLs

Once the containers are running, access the applications using these URLs:

| Application | URL | Port |
|---|---|---|
| OLTP Web Application | http://localhost:8080 | 8080 |
| OLAP API | http://localhost:8081/api/reports/missed-visits-by-month | 8081 |
| React Frontend | http://localhost:3000 | 3000 |
| SQL Server | `localhost,14331` | 14331 |

**Note:** SQL Server is accessed through a database client such as SQL Server Management Studio (SSMS), not a web browser. Use the `sa` login and the `SA_PASSWORD` from your `.env`.

### 5. View Container Logs
```powershell
docker compose logs -f
```

### 6. Start Containers in the Background
```powershell
docker compose up -d
```

### 7. Stop All Containers
```powershell
docker compose down
```

### 8. Rebuild After Code Changes
```powershell
docker compose up --build
```
