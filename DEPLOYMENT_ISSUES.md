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

## Issue 7: Frontend Build Context Path

### Problem
Docker Compose couldn't find the frontend source code because of incorrect build context path.

### Errors
```
ERROR: build path /root/bohucopos/BohucoPos-FrontEnd/NexusPOS.Frontend either does not exist, is not accessible, or is not a valid URL
```

### Resolution
- Use `../../BohucoPos.FrontEnd` (not `../../BohucoPos-FrontEnd/NexusPOS.Frontend`)
- The path is relative from `deploy/docker-compose.yml`

### Folder Structure Expected
```
/opt/
├── BohucoPost/
│   └── deploy/
│       └── docker-compose.yml
└── BohucoPos.FrontEnd/    ← Note: no hyphen, no NexusPOS.Frontend subfolder
    └── (frontend files)
```

---

## Issue 8: SignalR Connection Failed (Hardcoded URL)

### Problem
Frontend was trying to connect to SignalR at hardcoded `https://localhost:7089` instead of using the proxy.

### Errors
```
POST https://localhost:7089/hubs/orders/negotiate net::ERR_CONNECTION_REFUSED
Failed to complete negotiation with the server: TypeError: Failed to fetch
```

### Resolution
- Updated `.env.development` with relative URLs:
  ```
  VITE_API_URL=/api
  VITE_SIGNALR_URL=/hubs/orders
  ```
- Updated `src/config.ts`: `const API_URL = import.meta.env.VITE_API_URL || '';`

### Files Modified
- `NexusPOS.Frontend/.env.development`
- `NexusPOS.Frontend/src/config.ts`

---

## Issue 9: Duplicate /api/api/ Path

### Problem
API requests were going to `/api/api/auth/login` instead of `/api/auth/login`.

### Errors
```
POST http://localhost/api/api/auth/login 404 (Not Found)
```

### Resolution
- Changed `config.ts` default from `'/api'` to `''`:
  ```typescript
  const API_URL = import.meta.env.VITE_API_URL || '';
  ```

### Files Modified
- `NexusPOS.Frontend/src/config.ts`

---

## Issue 10: Nginx Proxy Path Duplication

### Problem
Nginx was stripping `/api` prefix when proxying to backend, causing 404s.

### Errors
- Request: `/api/auth/login`
- Backend received: `/auth/login` (missing /api)

### Resolution
- Updated `deploy/nginx.conf`:
  ```nginx
  location /api/ {
      proxy_pass http://api_backend/api/;  # Keep /api/ prefix
  }
  ```

### Files Modified
- `deploy/nginx.conf`

---

## Issue 11: Seq Port 5341 Already in Use (Local Development)

### Problem
Seq container couldn't start because port 5341 was already in use on the local machine (Windows).

### Errors
```
Ports are not available: exposing port TCP 0.0.0.0:5341 -> 0.0.0.0:0: listen tcp 0.0.0.0:5341: bind: An attempt was made to access a socket in a way forbidden by its access permissions.
```

### Resolution
- For local development: Use `docker logs nexuspos-api -f` instead of Seq
- For production: Seq will work because port 5341 is free on the droplet
- Alternative: Use `SEQ_URL` environment variable to connect to external Seq

### Configuration
```yaml
# docker-compose.yml
environment:
  Seq__Url: ${SEQ_URL:-http://nexuspos-seq:5341}
```

---

## Issue 12: Database Data Lost on Container Destroy

### Problem
Database data is lost when containers are destroyed because volumes need to be persisted.

### Resolution
- **DO NOT use** `docker-compose down -v` (the `-v` flag removes volumes)
- **USE** `docker-compose down` without `-v` to preserve data
- **OR USE** `docker-compose stop` and `docker-compose start` to restart without destroying

### Important Commands
```bash
# ✅ Correct - keeps database data
docker-compose down
docker-compose stop
docker-compose start

# ❌ Wrong - deletes database data
docker-compose down -v
```

### Volume Configuration
The database already has a persistent volume:
```yaml
volumes:
  - nexuspos_docker_data:/var/lib/postgresql/data
```

### Problem
Seq container couldn't start because port 5341 was already in use on the local machine (Windows).

### Errors
```
Ports are not available: exposing port TCP 0.0.0.0:5341 -> 0.0.0.0:0: listen tcp 0.0.0.0:5341: bind: An attempt was made to access a socket in a way forbidden by its access permissions.
```

### Resolution
- For local development: Use `docker logs nexuspos-api -f` instead of Seq
- For production: Seq will work because port 5341 is free on the droplet
- Alternative: Use `SEQ_URL` environment variable to connect to external Seq

### Configuration
```yaml
# docker-compose.yml
environment:
  Seq__Url: ${SEQ_URL:-http://nexuspos-seq:5341}
```

---

## Final Working Configuration (March 24, 2026 - Updated)

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
| nexuspos-seq | 5341 | Seq Logging Server (production only |

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
