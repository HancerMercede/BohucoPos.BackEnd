# BOHUCO POS - Deployment Guide

All-in-one droplet deployment with Docker Compose.

## Prerequisites

1. **Droplet** (Ubuntu 22.04+)
   - Minimum 2GB RAM, 2 vCPU
   - Open ports: 80, 443, 22 (SSH)

2. **Domain** (optional but recommended)
   - Point A record to your droplet IP

---

## Quick Start

### 1. Clone Repositories

```bash
# Clone backend
cd /opt
git clone https://github.com/your-org/BohucoPost.git
cd BohucoPost

# Clone frontend
cd /opt
git clone https://github.com/your-org/BohucoPos-FrontEnd.git
```

### 2. Configure Environment

```bash
cd /opt/BohucoPost/deploy
cp .env.production .env

# Edit .env with your values
nano .env
```

Required values:
- `DB_PASSWORD` - Secure password for database
- `JWT_KEY` - Generate with: `openssl rand -base64 32`
- `API_URL` - Your domain (e.g., https://pos.yourrestaurant.com)

### 3. Set Up Docker

```bash
# Install Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/download/v2.24.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### 4. Deploy

```bash
cd /opt/BohucoPost/deploy

# Create required directories
mkdir -p ssl

# Start services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f
```

### 5. Set Up SSL (Let's Encrypt)

```bash
# Install Certbot
sudo apt update
sudo apt install certbot python3-certbot-nginx

# Get certificate (replace with your domain)
sudo certbot --nginx -d pos.yourrestaurant.com

# Cert auto-renewal
sudo certbot renew --dry-run
```

### 6. Verify

- Frontend: http://your-domain.com
- API: http://your-domain.com/api/tabs/active
- Health: http://your-domain.com/health

---

## Management Commands

```bash
# View logs
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f postgres

# Restart services
docker-compose restart

# Update and redeploy
cd /opt/BohucoPost
git pull
docker-compose build
docker-compose up -d

# Backup database
docker-compose exec postgres pg_dump -U nexuspos nexuspos > backup_$(date +%Y%m%d).sql

# Stop everything
docker-compose down
```

---

## Project Structure

```
/opt/BohucoPost/
├── deploy/
│   ├── docker-compose.yml    # Container orchestration
│   ├── nginx.conf            # Reverse proxy config
│   ├── .env.production       # Environment variables template
│   └── README.md             # This file
├── NexusPOS.API/             # Backend source
└── NexusPOS.sln

/opt/BohucoPos-FrontEnd/
└── NexusPOS.Frontend/        # Frontend source
```

---

## Troubleshooting

### Container won't start
```bash
# Check logs
docker-compose logs [service-name]

# Common issues:
# - Port already in use: check nginx/apache on port 80
# - Database connection: verify DB credentials in .env
```

### Database issues
```bash
# Reset database
docker-compose down -v
docker-compose up -d

# Connect to database
docker-compose exec postgres psql -U nexuspos
```

### SSL issues
```bash
# Check certbot
sudo certbot certificates

# Renew manually
sudo certbot renew
```

---

## First-Time Setup

After first deployment:

1. Register admin user via frontend
2. Add products via Admin panel
3. Configure table/bar layout (future feature)

---

## Historial de Cambios / Traceability

### 2026-03-24 - Seq Observability Integration

**Objetivo**: Agregar observabilidad centralized con Seq

**Cambios realizados**:

| Componente | Archivo | Descripción |
|------------|---------|-------------|
| Backend | `NexusPOS.API/Program.cs` | + Serilog + Seq config, request logging middleware |
| Backend | `NexusPOS.API/Helpers/ServiceExtensions.cs` | + OrderHub using, hub options |
| Backend | `NexusPOS.API/NexusPOS.API.csproj` | + Serilog.AspNetCore, Serilog.Sinks.Seq packages |
| Backend | `NexusPOS.API/appsettings.json` | + Seq config (production) |
| Backend | `NexusPOS.API/appsettings.Development.json` | + Seq config (development) |
| Frontend | `NexusPOS.Frontend/src/config.ts` | API_URL default `''` para evitar `/api/api/` |
| Frontend | `NexusPOS.Frontend/.env.development` | URLs relativas `/api`, `/hubs/orders` |
| Deploy | `deploy/docker-compose.yml` | + Seq service, Seq env var |
| Deploy | `deploy/nginx.conf` | Fix `/api/` proxy path |

**Commits**:
- `3168abd` - Add Seq observability with Serilog integration (backend)
- `3299677` - Fix API URL config for Docker deployment (frontend)

**Ramas**:
- Backend: `feature/seq-observability` → `development`
- Frontend: `main`

### Issues Resueltos
- SignalR connection failed (7089 hardcoded URL en build)
- `/api/api/` duplicate path (API_URL config)
- Nginx proxy path incorrecto

---

## Observability (Seq)

Seq está configurado para centralized logging.

### Acceso en Producción
```
http://<droplet-ip>:5341
```

### Ver Logs en Desarrollo Local
```bash
docker logs nexuspos-api -f
```

### Configuración
- **Producción**: Seq corre en contenedor Docker (`nexuspos-seq:5341`)
- **Desarrollo**: Usar `docker logs` directamente

---

## Cost Estimate

| Item | Monthly Cost |
|------|--------------|
| Droplet (2GB/2CPU) | ~$15 |
| Domain | ~$10/year |
| **Total** | ~$15-20/month |
