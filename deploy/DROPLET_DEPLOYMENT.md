# BOHUCO POS - Deployment a Droplet (Pasos Detallados)

## Información del Servidor

| Item | Valor |
|------|-------|
| IP Droplet | 134.209.165.233 |
| Usuario | root |
| RAM | 1GB |
| SSD | 25GB |

---

## Estructura de Carpetas Requerida

La estructura debe ser:

```
/root/bohucopos/
├── BohucoPos.BackEnd/
│   └── deploy/
│       └── docker-compose.yml
└── BohucoPos.FrontEnd/
    ├── Dockerfile
    ├── nginx.conf
    ├── package.json
    ├── yarn.lock
    └── src/
```

---

## Paso 1: Conectar al Droplet

```bash
ssh root@134.209.165.233
```

---

## Paso 2: Instalar Docker (si no está instalado)

```bash
# Instalar Docker
curl -fsSL https://get.docker.com | sh

# Iniciar y habilitar Docker
systemctl start docker
systemctl enable docker

# Verificar instalación
docker --version
docker-compose --version
```

---

## Paso 3: Crear Estructura de Carpetas y Clonar Repos

```bash
# Ir a home
cd /root

# Crear directorio
mkdir -p bohucopos
cd bohucopos

# Clonar repositorios (usar main branch)
git clone https://github.com/HancerMercede/BohucoPos.BackEnd.git
git clone https://github.com/HancerMercede/BohucoPos.FrontEnd.git

# Verificar estructura
ls -la
```

**Resultado esperado:**
```
bohucopos/
├── BohucoPos.BackEnd/
│   ├── deploy/
│   │   └── docker-compose.yml
│   └── ...
└── BohucoPos.FrontEnd/
    ├── Dockerfile
    └── ...
```

---

## Paso 4: Verificar docker-compose.yml

Asegúrate de que el archivo tenga las rutas correctas. El archivo en GitHub ya está configurado, pero verifica que tenga:

```yaml
services:
  api:
    build:
      context: ..
      dockerfile: NexusPOS.API/Dockerfile

  frontend:
    build:
      context: ../../BohucoPos.FrontEnd
      dockerfile: Dockerfile
```

---

## Paso 5: Ejecutar Docker Compose

```bash
# Ir a la carpeta de deploy
cd /root/bohucopos/BohucoPos.BackEnd/deploy

# Si hay imágenes corrupta, eliminar todo primero:
docker-compose down -v --rmi all
docker system prune -af

# Construir y levantar servicios
docker-compose up -d --build

# Ver estado de servicios
docker-compose ps
```

**Nota:** Las migraciones de base de datos se ejecutan automáticamente al iniciar la API (no necesitas ejecutarlas manualmente).

---

## Paso 6: Verificar que los Servicios Estén Corriendo

```bash
# Ver todos los contenedores
docker ps

# Ver logs de nginx
docker logs nexuspos-nginx

# Ver logs de API
docker logs nexuspos-api

# Ver logs de base de datos
docker logs nexuspos-db-docker
```

**Contenedores esperados:**
| Nombre | Estado |
|--------|--------|
| nexuspos-nginx | Running |
| nexuspos-api | Running |
| nexuspos-frontend | Running |
| nexuspos-db-docker | Running (healthy) |

---

## Paso 7: Aplicar Migraciones de Base de Datos

```bash
# Instalar .NET (si no está)
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Agregar al PATH
export PATH="$HOME/.dotnet:$PATH"

# Verificar
dotnet --version

# Aplicar migraciones
cd /root/bohucopos/BohucoPos.BackEnd

dotnet ef database update --connection "Host=nexuspos-db;Database=nexuspos;Username=nexuspos;Password=temppassword123;Port=5432"
```

---

## Paso 8: Verificar Funcionamiento

```bash
# Verificar nginx (health check)
curl http://localhost:80/health

# Verificar API a través de nginx
curl http://localhost:80/api/products

# Verificar frontend
curl http://localhost:80/

# Verificar desde fuera (reemplazar con tu IP)
curl http://134.209.165.233:80/health
```

**Respuestas esperadas:**
- `/health` → `healthy`
- `/api/products` → `401` (necesita auth, eso es correcto)
- `/` → HTML del frontend

---

## Comandos Útiles de Docker Compose

```bash
# Ver servicios
docker-compose ps

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f nginx
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f nexuspos-db-docker

# Reiniciar un servicio
docker-compose restart api
docker-compose restart nginx

# Detener todos los servicios
docker-compose down

# Detener y eliminar volúmenes (CUIDADO: borra datos)
docker-compose down -v

# Reconstruir y levantar
docker-compose up -d --build
```

---

## Verificar Comunicación a través de Nginx

Los logs de nginx deben mostrar las peticiones API:

```bash
docker logs nexuspos-nginx --tail 20 -f
```

Deberías ver entradas como:
```
GET /api/products HTTP/1.1" 200
GET /api/orders/pending/0 HTTP/1.1" 200
```

---

## Puertos Expuestos

| Servicio | Puerto | URL |
|----------|--------|-----|
| Nginx (Proxy) | 80 | http://134.209.165.233:80 |
| API | 5001 | http://134.209.165.233:5001 |
| Frontend | 8081 | http://134.209.165.233:8081 |
| PostgreSQL | 5436 | localhost:5436 |

---

## Solución de Problemas

### Error: "build path does not exist"
- Verificar estructura de carpetas
- Asegurarse de que ambos repos están clonados en `/root/bohucopos/`

### Error: "lockfile needs to be updated"
- Ya está solucionado en el Dockerfile (usa `yarn install` en lugar de `--frozen-lockfile`)

### Error: "connection refused" a la base de datos
```bash
# Verificar que postgres está corriendo
docker ps | grep postgres

# Ver logs
docker logs nexuspos-db-docker
```

### API no responde
```bash
# Ver logs de API
docker logs nexuspos-api

# Verificar variables de entorno
docker exec nexuspos-api env
```

### Frontend no carga
```bash
# Verificar que el contenedor está corriendo
docker ps | grep frontend

# Ver logs
docker logs nexuspos-frontend
```

---

## Actualizar el Deployment (Cuando hay cambios en GitHub)

```bash
# Conectar al droplet
ssh root@134.209.165.233

# Ir a las carpetas y hacer pull
cd /root/bohucopos/BohucoPos.BackEnd
git pull origin development

cd /root/bohucopos/BohucoPos.FrontEnd  
git pull origin main  # o development

# Reconstruir y levantar
cd /root/bohucopos/BohucoPos.BackEnd/deploy
docker-compose up -d --build
```

---

## Datos de Conexión de la Base de Datos

| Variable | Valor |
|----------|-------|
| Host | nexuspos-db |
| Puerto | 5432 |
| Database | nexuspos |
| Usuario | nexuspos |
| Password | temppassword123 |

---

## Checklist de Verificación

- [ ] Conexión SSH exitosa
- [ ] Docker instalado y corriendo
- [ ] Repos clonados en estructura correcta
- [ ] `docker-compose up -d --build` ejecutado sin errores
- [ ] Todos los contenedores corriendo (`docker ps`)
- [ ] Migraciones aplicadas
- [ ] Health check respondiendo (`curl localhost:80/health`)
- [ ] API respondiendo a través de nginx
- [ ] Frontend cargando desde navegador
- [ ] Verificado desde máquina externa (http://134.209.165.233:80)

---

## URLs de la Aplicación

| Servicio | URL |
|----------|-----|
| App Principal | http://134.209.165.233:80 |
| API (directo) | http://134.209.165.233:5001 |
| Frontend (directo) | http://134.209.165.233:8081 |

---

## Contacto de Soporte

Si hay errores no resueltos, revisar:
1. Logs de docker: `docker-compose logs -f`
2. Estado de contenedores: `docker ps`
3. Recursos del servidor: `docker stats`
