# URL Shortener Application
## Microservices Architecture

A production-ready URL shortener built with microservices architecture using ASP.NET Core 8.0, Vue.js 3, PostgreSQL, Redis, and Docker.

---

## Features

- ✅ User authentication with JWT tokens
- ✅ URL shortening with custom aliases
- ✅ Click tracking and analytics
- ✅ Admin dashboard for monitoring
- ✅ RESTful API design with API Gateway
- ✅ Redis caching for performance
- ✅ Fully containerized with Docker
- ✅ CI/CD pipeline with GitHub Actions

---

## Architecture

```
┌─────────────┐
│   Client    │
│  (Browser)  │
└──────┬──────┘
       │ HTTP/HTTPS
       ▼
┌─────────────────────┐
│   API Gateway       │
│   (Ocelot :5000)    │
└──┬──────────────┬───┘
   │              │
   ▼              ▼
┌──────────┐  ┌──────────────┐
│  User    │  │  Shortener   │
│ Service  │  │   Service    │
│  :5001   │  │    :5126     │
└────┬─────┘  └──────┬───────┘
     │               │
     ▼               ▼
  UserDB        ShortUrlDB
 (Postgres)     (Postgres)
```

### Services

- **UserService** (Port 5001) - Authentication and user management
- **ShortenerService** (Port 5126) - URL shortening and analytics
- **ApiGateway** (Port 5000) - Request routing and JWT validation
- **Frontend** (Port 3000) - Vue.js 3 SPA

---

## Tech Stack

**Backend:**
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- Ocelot API Gateway
- JWT Authentication
- MediatR (CQRS)

**Frontend:**
- Vue.js 3
- Vite
- Vue Router

**Database & Caching:**
- PostgreSQL 15
- Redis
- RabbitMQ

**DevOps:**
- Docker & Docker Compose
- GitHub Actions
- Railway PaaS

---

## Quick Start

### Prerequisites

- Docker & Docker Compose
- .NET 8.0 SDK (for local development)
- Node.js 18+ (for frontend development)

### Run with Docker Compose

```bash
# Clone repository
git clone https://github.com/ducanhnguyen223/AMD201_Shorted_URL.git
cd AMD201_Shorted_URL

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f
```

### Access Application

- **Frontend:** http://localhost:3000
- **API Gateway:** http://localhost:5000
- **UserService Swagger:** http://localhost:5001
- **ShortenerService Swagger:** http://localhost:5126

### Default Admin Credentials

```
Email: admin@admin.com
Password: admin123
```

---

## Local Development

### Backend Services

```bash
# UserService
cd UserService
dotnet restore
dotnet run

# ShortenerService
cd ShortenerService
dotnet restore
dotnet run

# ApiGateway
cd ApiGateway
dotnet restore
dotnet run
```

### Frontend

```bash
cd vue-project
npm install
npm run dev
```

---

## Environment Variables

### UserService

```env
ConnectionStrings__DefaultConnection=Host=localhost;Database=UserDB;Username=postgres;Password=postgres
JwtSettings__Secret=YOUR_SECRET_KEY
JwtSettings__Issuer=ShortenerApp
JwtSettings__Audience=ShortenerApp.Client
JwtSettings__ExpiryMinutes=60
```

### ShortenerService

```env
ConnectionStrings__DefaultConnection=Host=localhost;Database=ShortUrlDB;Username=postgres;Password=postgres
ConnectionStrings__RedisConnection=localhost:6379
JwtSettings__Secret=YOUR_SECRET_KEY
JwtSettings__Issuer=ShortenerApp
JwtSettings__Audience=ShortenerApp.Client
RabbitMQ__HostName=localhost
RabbitMQ__Port=5672
RabbitMQ__UserName=guest
RabbitMQ__Password=guest
```

### ApiGateway

```env
JwtSettings__Secret=YOUR_SECRET_KEY
JwtSettings__Issuer=ShortenerApp
JwtSettings__Audience=ShortenerApp.Client
ServiceHosts__UserService=http://localhost:5001
ServiceHosts__ShortenerService=http://localhost:5126
```

---

## API Endpoints

### Authentication

```bash
# Register
POST /api/auth/register
{
  "Email": "user@example.com",
  "Password": "password123",
  "FullName": "John Doe"
}

# Login
POST /api/auth/login
{
  "Email": "user@example.com",
  "Password": "password123"
}
```

### URL Shortening

```bash
# Create shortened URL
POST /api/urls
Authorization: Bearer {token}
{
  "OriginalUrl": "https://example.com/very-long-url",
  "CustomAlias": "mylink"  // optional
}

# Get user's URLs
GET /api/urls
Authorization: Bearer {token}

# Redirect to original URL
GET /{shortCode}

# Delete URL
DELETE /api/urls/{id}
Authorization: Bearer {token}
```

### Admin Dashboard

```bash
# Get statistics
GET /api/admin/stats
Authorization: Bearer {admin-token}

# Get all URLs
GET /api/admin/urls
Authorization: Bearer {admin-token}

# Get all users
GET /api/admin/users
Authorization: Bearer {admin-token}

# Delete any URL
DELETE /api/admin/urls/{id}
Authorization: Bearer {admin-token}

# Delete user
DELETE /api/admin/users/{id}
Authorization: Bearer {admin-token}
```

---

## Docker Images

All services are available on Docker Hub:

- Frontend: `ducanh223/amd201-frontend:latest`
- API Gateway: `ducanh223/amd201-api-gateway:latest`
- User Service: `ducanh223/amd201-user-service:latest`
- Shortener Service: `ducanh223/amd201-shortener-service:latest`

---

## CI/CD Pipeline

This project uses GitHub Actions for automated builds and deployments:

1. Push to `master` branch triggers workflow
2. Docker images are built for all services
3. Images are tagged with `latest` and commit SHA
4. Images are pushed to Docker Hub
5. Railway automatically deploys updated services

### Workflow File

See `.github/workflows/ci-cd.yml` for pipeline configuration.

---

## Deployment

### Railway PaaS

The application is deployed on Railway:

- **Production URL:** https://amd201shortedurl-production.up.railway.app/
- **API Gateway Health:** https://api-gateway-production-e75a.up.railway.app/health

### Deploying to Railway

1. Create Railway account
2. Create new project from GitHub repo
3. Add PostgreSQL and Redis services
4. Configure environment variables
5. Deploy services

---

## Project Structure

```
AMD201_Shorted_URL/
├── ApiGateway/              # Ocelot API Gateway
├── UserService/             # User authentication service
├── ShortenerService/        # URL shortening service
├── vue-project/             # Vue.js frontend
├── .github/workflows/       # CI/CD pipeline
├── docker-compose.yml       # Local development setup
└── README.md
```

---

## Testing

### Health Checks

```bash
# API Gateway
curl http://localhost:5000/health

# UserService
curl http://localhost:5001/api/health

# ShortenerService
curl http://localhost:5126/api/health
```

### Example Usage Flow

```bash
# 1. Register new user
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"Email":"test@example.com","Password":"test123","FullName":"Test User"}'

# 2. Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Email":"test@example.com","Password":"test123"}'

# 3. Create shortened URL (use token from login response)
curl -X POST http://localhost:5000/api/urls \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"OriginalUrl":"https://github.com/ducanhnguyen223/AMD201_Shorted_URL"}'

# 4. Access short URL (get shortCode from previous response)
curl -L http://localhost:5000/{shortCode}
```

---

## Troubleshooting

### Docker Issues

```bash
# Remove all containers and volumes
docker-compose down -v

# Rebuild images
docker-compose build --no-cache

# View logs
docker-compose logs -f [service-name]
```

### Database Issues

```bash
# Connect to PostgreSQL
docker exec -it postgres psql -U postgres

# List databases
\l

# Connect to database
\c UserDB

# List tables
\dt
```

### Redis Issues

```bash
# Connect to Redis
docker exec -it redis redis-cli

# Check keys
KEYS *

# Get key value
GET ShortenerService_shortcode
```

---

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

---

## License

This project is for educational purposes as part of AMD201 course.

---

## Contact

- GitHub: [@ducanhnguyen223](https://github.com/ducanhnguyen223)
- Project Link: https://github.com/ducanhnguyen223/AMD201_Shorted_URL
- Live Demo: https://amd201shortedurl-production.up.railway.app/

---

## Acknowledgments

- FPT University - AMD201 Course
- ASP.NET Core Documentation
- Vue.js Documentation
- Docker Documentation
- Railway Platform
