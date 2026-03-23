# BOHUCO POS - Deployment Guide

## Overview

This document describes the complete deployment setup for BOHUCO POS system using Docker Compose.

---

## Architecture

```
                           ┌─────────────────────┐
                           │     Nginx Proxy     │
                           │      (Port 80)      │
                           └──────────┬──────────┘
                                      │
              ┌───────────────────────┼───────────────────────┐
              │                       │                       │
              ▼                       ▼                       ▼
    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
    │   Frontend      │    │      API        │    │   PostgreSQL    │
    │  (React+Nginx)  │    │    (.NET 10)    │    │   (Port 5436)   │
    │   (Port 8081)   │    │  (Port 5001)    │    │                 │
    └─────────────────┘    └─────────────────┘    └─────────────────┘
```

---

## Services

| Service | Container Name | Port (External) | Port (Internal) | Description |
|---------|---------------|-----------------|-----------------|-------------|
| nexuspos-db-docker | nexuspos-db-docker | 5436 | 5432 | PostgreSQL Database |
| nexuspos-api | nexuspos-api | 5001 | 8080 | .NET Backend API |
| nexuspos-frontend | nexuspos-frontend | 8081 | 80 | React Frontend |
| nexuspos-nginx | nexuspos-nginx | 80 | 80 | Reverse Proxy |

---

## Configuration

### Database Credentials

| Variable | Value |
|----------|-------|
| POSTGRES_USER | nexuspos |
| POSTGRES_PASSWORD | temppassword123 |
| POSTGRES_DB | nexuspos |
| Port | 5436 |

### Connection Strings

**Local Development (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5435"
  }
}
```

**Docker Deployment (auto-configured):**
```json
{
  "ConnectionStrings": {
    "DockerConnection": "Host=nexuspos-db;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5432"
  }
}
```

### API Endpoints

| Endpoint | Description |
|----------|-------------|
| `/api/auth/*` | Authentication (login, register) |
| `/api/products/*` | Products CRUD |
| `/api/orders/*` | Orders management |
| `/api/tabs/*` | Table/Tab management |
| `/api/dashboard/*` | Dashboard analytics |
| `/api/pdf/*` | PDF generation |
| `/hubs/orders` | SignalR real-time |

---

## Traffic Flow

### Through Nginx (Port 80) - Recommended
```
User Browser
    │
    ▼
http://localhost:80
    │
    ├── /api/* ──────────────▶ nginx ──▶ API:8080
    ├── /hubs/* ────────────▶ nginx ──▶ API:8080 (WebSocket)
    └── /* ─────────────────▶ nginx ──▶ Frontend:80
```

### Direct Access (Development)
- Frontend: `http://localhost:8081`
- API: `http://localhost:5001`
- Database: `localhost:5436`

---

## Files Structure

```
BohucoPos.BackEnd/
├── deploy/
│   ├── docker-compose.yml      # Main deployment config
│   ├── nginx.conf              # Nginx reverse proxy config
│   ├── .env                    # Environment variables
│   └── README.md               # Quick start guide
│
├── NexusPOS.API/
│   ├── Dockerfile             # API container definition
│   ├── appsettings.json        # Configuration
│   └── ...
│
└── NexusPOS.sln                # Solution file

BohucoPos.FrontEnd/
├── NexusPOS.Frontend/
│   ├── Dockerfile              # Frontend container
│   ├── nginx.conf              # Frontend nginx config
│   ├── .dockerignore
│   └── src/
│       └── config.ts           # API URL config
│
└── package.json
```

---

## Setup & Deployment

### Prerequisites
- Docker 20.10+
- Docker Compose 2.0+
- .NET 10 SDK (for migrations)
- Node.js 20+ (for frontend build via Docker)

### Quick Start (Local Development)

```bash
# 1. Clone the repository
git clone https://github.com/HancerMercede/BohucoPos.BackEnd.git
git clone https://github.com/HancerMercede/BohucoPos.FrontEnd.git

# 2. Navigate to deploy folder
cd BohucoPos.BackEnd/deploy

# 3. Start all services
docker-compose up -d --build

# 4. Apply database migrations
dotnet ef database update --connection "Host=localhost;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5436"

# 5. Verify services
docker-compose ps
curl http://localhost:80/health
```

### Production Deployment (Droplet)

```bash
# SSH to your droplet
ssh user@your-droplet-ip

# Install Docker if not installed
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Clone repositories
git clone https://github.com/HancerMercede/BohucoPos.BackEnd.git
git clone https://github.com/HancerMercede/BohucoPos.FrontEnd.git

# Navigate to deploy
cd BohucoPos.BackEnd/deploy

# Create environment file
cp .env.example .env
# Edit .env with your production values

# Start services
docker-compose up -d --build

# Apply migrations
dotnet ef database update --connection "Host=nexuspos-db;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5432"

# Check status
docker-compose ps
docker-compose logs -f
```

---

## Common Commands

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# Rebuild and start
docker-compose up -d --build

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f api
docker-compose logs -f nginx

# Execute commands in container
docker exec -it nexuspos-api bash
docker exec -it nexuspos-db-docker psql -U nexuspos -d nexuspos

# Restart specific service
docker-compose restart api

# Check service health
curl http://localhost:80/health
curl http://localhost:5001/api/products
```

---

## Troubleshooting

### API Cannot Connect to Database
```bash
# Check if database is running
docker-compose ps nexuspos-db-docker

# Check database logs
docker-compose logs nexuspos-db-docker

# Verify connection
docker exec -it nexuspos-db-docker psql -U nexuspos -d nexuspos -c "SELECT 1"
```

### Frontend Cannot Connect to API
```bash
# Check nginx logs
docker-compose logs nexuspos-nginx

# Verify API is running
docker-compose logs nexuspos-api

# Check network connectivity
docker network inspect deploy_nexuspos-network
```

### Migration Errors
```bash
# If tables already exist
dotnet ef database update --force

# Or drop and recreate (WARNING: loses data)
docker exec -it nexuspos-db-docker psql -U nexuspos -d nexuspos -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"
dotnet ef database update
```

---

## Completed Features

### ✅ Implemented
- [x] Docker Compose setup with all services
- [x] PostgreSQL database with persistence
- [x] .NET API containerized
- [x] React Frontend containerized
- [x] Nginx reverse proxy
- [x] Frontend → API communication through nginx
- [x] SignalR WebSocket support
- [x] Health check for database
- [x] Database migrations
- [x] CORS configuration
- [x] Environment variables support

---

## Pending / Future Features

### 🔄 In Progress
- [ ] SSL/HTTPS configuration
- [ ] Environment-specific configurations (staging, production)

### 📋 To Do
- [ ] CI/CD pipeline setup (GitHub Actions)
- [ ] Database backup strategy
- [ ] Log aggregation (ELK/Loki)
- [ ] Monitoring (Prometheus/Grafana)
- [ ] Auto-scaling configuration
- [ ] Database connection pooling optimization

---

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `DB_CONNECTION` | (see docker-compose) | Full connection string |
| `API_URL` | http://localhost:5001 | API endpoint for frontend build |
| `ASPNETCORE_ENVIRONMENT` | Production | .NET environment |

---

## Security Notes

1. **Change default passwords** before production deployment
2. **Enable SSL** using Let's Encrypt or your own certificate
3. **Restrict database access** to internal network only
4. **Use secrets management** for production (Docker secrets, HashiCorp Vault)
5. **Keep Docker images updated** for security patches

---

## Support

For issues or questions:
- Check logs: `docker-compose logs -f`
- Verify services: `docker-compose ps`
- Test endpoints: `curl http://localhost:80/health`
