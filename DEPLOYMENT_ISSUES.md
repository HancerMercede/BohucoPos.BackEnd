# BOHUCO POS - Deployment Issues Historical Record

## Date: March 23, 2026

---

## Issue 1: Docker Compose Deployment - Database Connection Issues

### Problem
API was configured to connect to wrong port and host when running in Docker.

### Errors
- API tried to connect to `localhost:5434` instead of Docker service name `postgres`
- Wrong port configuration in appsettings.json

### Resolution
- Updated appsettings.json to use correct Docker network configuration
- For local development: `Host=localhost;Port=5435`
- For Docker: `Host=postgres;Port=5432`

---

## Issue 2: CORS Configuration

### Problem
Frontend running on port 8081 (Docker) was blocked by CORS policy.

### Errors
```
Access to fetch at 'http://localhost:5001/api/auth/login' from origin 'http://localhost:8081' 
has been blocked by CORS policy
```

### Resolution
- Added port 8081 to allowed origins in `NexusPOS.API/Helpers/ServiceExtensions.cs`

---

## Issue 3: Hardcoded Connection String in DbContext

### Problem
Connection string was hardcoded in `AppDbContext.cs` and `AppDbContextFactory.cs`, bypassing appsettings.json (bad practice).

### Errors
```
Failed to connect to 127.0.0.1:5434
```

### Resolution
- Updated `AppDbContextFactory.cs` to read connection string from appsettings.json
- Used manual JSON parsing to avoid adding extra dependencies to Infrastructure project

---

## Issue 4: Database Port Confusion

### Problem
Multiple PostgreSQL containers were running on different ports causing confusion.

### Current Setup
| Container | Port | User | Password |
|-----------|------|------|----------|
| stockmonitor-postgres-local | 5432 | postgres | postgres |
| tradingjournal-db | 5433 | postgres | postgres |
| nexuspos-db | 5435 | nexuspos | temppassword123 |

### Resolution
- Used `nexuspos-db` on port 5435
- Connection string: `Host=localhost;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5435`

---

## Issue 5: Database Container Removed

### Problem
When cleaning up containers, the nexuspos-db container was deleted, losing the database.

### Resolution
- Recreated nexuspos-db container on port 5435
- Ran migrations: `dotnet ef database update`

---

## Issue 6: Hardcoded Credentials in Files (Earlier)

### Problem
Password authentication kept failing due to wrong credentials.

### Errors
```
password authentication failed for user "nexuspos"
```

### Resolution
- Verified correct credentials from Docker environment variables
- Updated appsettings.json with correct values

---

## Final Working Configuration (March 23, 2026 - Updated)

### Docker Deployment
| Service | Port | Description |
|---------|------|-------------|
| nexuspos-db-docker | 5436 | PostgreSQL for Docker deployment |
| nexuspos-api | 5001 | .NET API |
| nexuspos-frontend | 8081 | React Frontend |
| nexuspos-nginx | 80 | Reverse Proxy |

### Database Credentials
```
Container: nexuspos-db-docker
Port: 5436 (external), 5432 (internal)
Database: nexuspos
User: nexuspos
Password: temppassword123
```

### Connection Strings (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5435",
    "DockerConnection": "Host=nexuspos-db;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5432"
  }
}
```

### Tables Created in Docker DB
- OrderItems
- Orders
- Products
- Tabs
- Users
- __EFMigrationsHistory

### Running the Deployment
```bash
cd deploy
docker-compose up -d --build

# Apply migrations
dotnet ef database update --connection "Host=localhost;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5436"
```

---

## Lessons Learned

1. **Always use appsettings.json** for connection strings, never hardcode in DbContext
2. **Document database credentials** - multiple PostgreSQL instances made it confusing
3. **Use environment variables** for Docker deployments
4. **Keep track of ports** - multiple services on different ports causes confusion

---

## Files Modified

- `NexusPOS.API/appsettings.json` - Database connection string
- `NexusPOS.API/Helpers/ServiceExtensions.cs` - CORS configuration  
- `NexusPOS.Infrastructure/Data/AppDbContextFactory.cs` - Read from appsettings.json
- `NexusPOS.API/Dockerfile` - API container definition
- `deploy/docker-compose.yml` - Docker Compose configuration

---

## Files Created

- `NexusPOS.sln` - Solution file for Docker build
- `deploy/docker-compose.yml` - Local deployment configuration
- `deploy/nginx.conf` - Reverse proxy configuration
- `deploy/.env` - Environment variables
- `deploy/.env.production` - Production environment template
- `deploy/README.md` - Deployment documentation
