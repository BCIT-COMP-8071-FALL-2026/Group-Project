# Docker Setup Steps

Run the following commands from the folder containing `docker-compose.yml`.

### 1. Build Docker Images
```powershell
docker compose build
```

### 2. Start All Containers
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
| OLAP API | http://localhost:8081/api/senior-visits | 8081 |
| React Frontend | http://localhost:3000 | 3000 |
| SQL Server | `localhost,14331` | 14331 |

**Note:** SQL Server is accessed through a database client such as SQL Server Management Studio (SSMS), not a web browser. Use the `sa` login and the password configured in `docker-compose.yml`.

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