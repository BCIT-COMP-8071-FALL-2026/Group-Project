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
On the first start, the `olap-db-init` service creates the OLAP database (`OlapDW`) from the scripts in `olap/sql`, then exits. Later starts keep the existing tables and data. The OLTP app sets up its own tables through migrations.

The report will be empty until visit data is added.

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

### 9. Reset the Databases (Refresh Seed Data)
The OLTP seed (#56) sets visit times relative to when it first runs and never reseeds, so the in-progress and upcoming visits go stale (they pass or turn Missed) after a few days. Remove the volume to start over:
```powershell
docker compose down -v
docker compose up --build
```
**Note:** `-v` deletes all data in both OLTPAppDb and OlapDW, including visits and notes created in the app. Both databases are rebuilt and reseeded on the next start. Reset before every demo.
