# URL Shortener Microservices - Docker Setup

## 🐳 Quick Start

### Prerequisites
- Docker Desktop installed
- Docker Compose installed
- Ports available: 1433, 5672, 15672, 6379, 5000, 5126, 3000

### 1. Build and Run All Services

```bash
# Build and start all services
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d
```

### 2. Wait for Services to be Ready

The services will start in this order:
1. **SQL Server** (port 1433) - ~10-30 seconds
2. **Redis** (port 6379) - ~5 seconds
3. **RabbitMQ** (ports 5672, 15672) - ~10 seconds
4. **Shortener Service** (port 5126) - after infrastructure is healthy
5. **API Gateway** (port 5000) - after Shortener Service
6. **Frontend** (port 3000) - after API Gateway

### 3. Access the Application

- **Frontend**: http://localhost:3000
- **API Gateway**: http://localhost:5000
- **Shortener Service (Direct)**: http://localhost:5126
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### 4. Test the API

```bash
# Create a short URL (via Gateway)
curl -X POST http://localhost:5000/api/shorten \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://www.google.com"}'

# Response example:
# {
#   "originalUrl": "https://www.google.com",
#   "shortUrl": "http://localhost:5000/abc123",
#   "shortCode": "abc123"
# }

# Access the short URL (redirect)
curl -L http://localhost:5000/abc123
```

## 🛠️ Docker Commands

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api-gateway
docker-compose logs -f shortener-service
docker-compose logs -f frontend
```

### Stop Services
```bash
# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: deletes all data)
docker-compose down -v
```

### Rebuild a Specific Service
```bash
# Rebuild only ShortenerService
docker-compose up --build shortener-service

# Rebuild only ApiGateway
docker-compose up --build api-gateway

# Rebuild only Frontend
docker-compose up --build frontend
```

### Check Service Health
```bash
# Check if all containers are running
docker-compose ps

# Check specific service health
docker inspect --format='{{.State.Health.Status}}' sqlserver
docker inspect --format='{{.State.Health.Status}}' redis
docker inspect --format='{{.State.Health.Status}}' rabbitmq
```

## 🏗️ Architecture

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ http://localhost:3000
       ▼
┌─────────────┐
│  Frontend   │ (Vue.js + Nginx)
│  Port: 3000 │
└──────┬──────┘
       │ API calls
       ▼
┌──────────────┐
│ API Gateway  │ (Ocelot)
│  Port: 5000  │
└──────┬───────┘
       │
       ├─────────────────┐
       ▼                 ▼
┌──────────────┐   ┌─────────────┐
│  Shortener   │   │    User     │
│   Service    │   │   Service   │
│  Port: 5126  │   │  (Future)   │
└──────┬───────┘   └─────────────┘
       │
       ├────┬────┬────┐
       ▼    ▼    ▼    ▼
    ┌────┐┌────┐┌────┐
    │SQL ││Redis││MQ │
    └────┘└────┘└────┘
```

## 📁 Service Ports

| Service | Port | Description |
|---------|------|-------------|
| Frontend | 3000 | Vue.js Web UI |
| API Gateway | 5000 | Ocelot Gateway |
| Shortener Service | 5126 | URL Shortening Service |
| SQL Server | 1433 | Database |
| Redis | 6379 | Cache |
| RabbitMQ | 5672 | Message Queue |
| RabbitMQ Management | 15672 | RabbitMQ Admin UI |

## 🔧 Environment Variables

### ShortenerService
- `ConnectionStrings__DefaultConnection`: SQL Server connection
- `ConnectionStrings__RedisConnection`: Redis connection
- `RabbitMQ__HostName`: RabbitMQ hostname
- `RabbitMQ__Port`: RabbitMQ port

### ApiGateway
- `JwtSettings__Secret`: JWT secret key
- `JwtSettings__Issuer`: JWT issuer
- `JwtSettings__Audience`: JWT audience

## 🐛 Troubleshooting

### Issue: Services fail to start
**Solution**: Ensure ports are not in use
```bash
# Windows
netstat -ano | findstr "5000"
netstat -ano | findstr "5126"
netstat -ano | findstr "3000"

# Kill process if needed (replace PID)
taskkill /PID <PID> /F
```

### Issue: Database connection fails
**Solution**: Wait for SQL Server to be healthy
```bash
# Check SQL Server logs
docker-compose logs sqlserver

# Restart ShortenerService after SQL Server is ready
docker-compose restart shortener-service
```

### Issue: "Connection refused" errors
**Solution**: Check service dependencies
```bash
# Ensure infrastructure is healthy first
docker-compose ps

# Check healthcheck status
docker inspect sqlserver | grep -A 5 Health
```

### Issue: Frontend can't connect to API
**Solution**: Check API Gateway is running
```bash
# Test API Gateway directly
curl http://localhost:5000/api/shorten

# Check API Gateway logs
docker-compose logs api-gateway
```

## 🎯 Next Steps

1. **Week 4**: Deploy to Cloud (Render/Azure/AWS)
2. **Add User Service**: Integrate authentication service
3. **CI/CD**: Setup GitHub Actions pipelines
4. **Monitoring**: Add health checks and logging

## 📝 Notes

- SQL Server password: `yourStrong(!)Password` (change in production!)
- RabbitMQ credentials: `guest/guest` (change in production!)
- All services are on the same Docker network: `shortener-network`
- Data persists in Docker volumes: `sqlserver_data`, `redis_data`, `rabbitmq_data`
