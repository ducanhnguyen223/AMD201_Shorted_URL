<div align="center">

# 🔗 URL Shortener Application

### A Modern Microservices-Based URL Shortening Platform

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Vue.js](https://img.shields.io/badge/Vue.js-3.0-4FC08D?style=for-the-badge&logo=vue.js&logoColor=white)](https://vuejs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-Alpine-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)

[Deployment status](#-deployment-status) • [API Docs](#api-documentation) • [Architecture](#architecture)

</div>

---

## 📋 Table of Contents

- [✨ Features](#-features)
- [🏗️ Architecture](#️-architecture)
- [🚀 Tech Stack](#-tech-stack)
- [⚡ Quick Start](#-quick-start)
- [🐳 Docker Deployment](#-docker-deployment)
- [📡 API Documentation](#-api-documentation)
- [🔐 Authentication](#-authentication)
- [🎯 Admin Dashboard](#-admin-dashboard)
- [🌐 Deployment status](#-deployment-status)
- [👥 Team](#-team)

---

## ✨ Features

<div align="center">

| Feature | Description |
|---------|-------------|
| 🔗 **URL Shortening** | Convert long URLs into short, shareable links |
| 🎯 **Custom Aliases** | Create personalized short codes for your URLs |
| 📊 **Analytics** | Track clicks and access statistics for each URL |
| 🔐 **Authentication** | JWT authentication with BCrypt password hashing |
| 👨‍💼 **Admin Dashboard** | Admin routes and a dashboard for user/URL management |
| ⚡ **Redis Caching** | Redis integration for URL lookup caching |
| 🐳 **Containerized** | Multi-service local setup with Docker Compose |
| 🔄 **Build workflow** | GitHub Actions build workflow with a GHCR image-publishing job |

</div>

---

## 🏗️ Architecture

### Microservices Design

```
┌─────────────────────────────────────────────────────────────┐
│                         Client (Browser)                     │
└────────────────────────────┬────────────────────────────────┘
                             │ HTTPS
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    🌐 API Gateway (Ocelot)                   │
│                         Port 5000                            │
└───────────────┬─────────────────────────┬───────────────────┘
                │                         │
        ┌───────▼────────┐        ┌──────▼─────────┐
        │  👥 User        │        │  🔗 Shortener  │
        │  Service        │        │   Service      │
        │  Port 5001      │        │   Port 5126    │
        └───────┬─────────┘        └──────┬─────────┘
                │                         │
        ┌───────▼─────────┐      ┌───────▼─────────┐
        │  💾 UserDB      │      │  💾 ShortUrlDB  │
        │  (PostgreSQL)   │      │  (PostgreSQL)   │
        └─────────────────┘      └────────┬────────┘
                                           │
                                  ┌────────▼────────┐
                                  │  ⚡ Redis Cache │
                                  └─────────────────┘
```

### Service Breakdown

**🌐 API Gateway** (Port 5000)
- Built with Ocelot
- Single entry point for all requests
- JWT token validation and forwarding
- CORS handling and request routing

**👥 User Service** (Port 5001)
- User authentication with JWT
- Registration and login management
- User profile management
- Admin user operations

**🔗 Shortener Service** (Port 5126)
- Core URL shortening logic
- Click tracking and analytics
- Custom alias support
- Redis caching integration
- Admin URL management

**🎨 Frontend** (Port 3000)
- Vue.js 3 SPA with Composition API
- Responsive design with Vite
- Environment-based configuration
- Admin dashboard with statistics

---

## 🚀 Tech Stack

### Backend

<div align="center">

| Technology | Version | Purpose |
|------------|---------|---------|
| ![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) | 8.0 | Web API framework |
| ![Entity Framework](https://img.shields.io/badge/Entity_Framework-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) | 8.0 | ORM for database access |
| ![Ocelot](https://img.shields.io/badge/Ocelot-Latest-FF6B6B?style=flat-square) | Latest | API Gateway |
| ![MediatR](https://img.shields.io/badge/MediatR-12.0-FFA500?style=flat-square) | 12.0 | CQRS pattern implementation |
| ![JWT](https://img.shields.io/badge/JWT-Bearer-000000?style=flat-square&logo=json-web-tokens&logoColor=white) | - | Authentication |

</div>

### Frontend

<div align="center">

| Technology | Version | Purpose |
|------------|---------|---------|
| ![Vue.js](https://img.shields.io/badge/Vue.js-3.0-4FC08D?style=flat-square&logo=vue.js&logoColor=white) | 3.0 | Frontend framework |
| ![Vite](https://img.shields.io/badge/Vite-5.0-646CFF?style=flat-square&logo=vite&logoColor=white) | 5.0 | Build tool |
| ![Vue Router](https://img.shields.io/badge/Vue_Router-4.0-4FC08D?style=flat-square&logo=vue.js&logoColor=white) | 4.0 | Client-side routing |

</div>

### Infrastructure

<div align="center">

| Technology | Version | Purpose |
|------------|---------|---------|
| ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-4169E1?style=flat-square&logo=postgresql&logoColor=white) | 15 | Primary database |
| ![Redis](https://img.shields.io/badge/Redis-Alpine-DC382D?style=flat-square&logo=redis&logoColor=white) | Alpine | Distributed caching |
| ![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-FF6600?style=flat-square&logo=rabbitmq&logoColor=white) | 3 | Message queue |
| ![Docker](https://img.shields.io/badge/Docker-Latest-2496ED?style=flat-square&logo=docker&logoColor=white) | Latest | Containerization |
| ![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI/CD-2088FF?style=flat-square&logo=github-actions&logoColor=white) | - | CI/CD pipeline |
| ![Railway](https://img.shields.io/badge/Railway-PaaS-0B0D0E?style=flat-square&logo=railway&logoColor=white) | - | Cloud hosting |

</div>

---

## ⚡ Quick Start

### Prerequisites

- Docker Desktop installed
- Git
- 4GB RAM minimum

### Local Development

```bash
# Clone the repository
git clone https://github.com/ducanhnguyen223/AMD201_Shorted_URL.git
cd AMD201_Shorted_URL

# Start all services with Docker Compose
docker-compose up -d

# Wait for services to be healthy (about 30 seconds)
docker-compose ps

# Access the application
# Frontend:    http://localhost:3000
# API Gateway: http://localhost:5000
# Postgres:    localhost:5432
# Redis:       localhost:6379
# RabbitMQ:    http://localhost:15672
```

### Demo accounts

No demo users are seeded by the repository. Registration creates a regular `User` account; the repository does not include an admin bootstrap flow. Admin routes require an account with the `Admin` role.

---

## 🐳 Docker Deployment

### Multi-Stage Dockerfile

All services use optimized multi-stage Docker builds:

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]
```

**Benefits:**
- ✅ Smaller image size (runtime only, no SDK)
- ✅ Consistent environments
- ✅ Fast builds with layer caching

### Docker Compose Services

```yaml
services:
  postgres:       # PostgreSQL database
  redis:          # Redis cache
  rabbitmq:       # Message queue
  user-service:   # User authentication service
  shortener-service: # URL shortening service
  api-gateway:    # Ocelot API Gateway
  frontend:       # Vue.js frontend
```

---

## 📡 API Documentation

### Base URL

```
Local:      http://localhost:5000
```

The previously listed Railway endpoint returned HTTP 404 on 2026-09-27; no live base URL is currently verified.

### Authentication Endpoints

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "Email": "user@example.com",
  "Password": "securePassword123",
  "FullName": "John Doe"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "Email": "user@example.com",
  "Password": "securePassword123"
}

Response:
{
  "Token": "eyJhbGciOiJIUzI1NiIs...",
  "Email": "user@example.com",
  "FullName": "John Doe"
}
```

### URL Shortening Endpoints

#### Create Short URL
```http
POST /api/urls
Authorization: Bearer {token}
Content-Type: application/json

{
  "OriginalUrl": "https://example.com/very/long/url",
  "CustomAlias": "mylink"  // Optional
}

Response:
{
  "ShortCode": "abc123",
  "OriginalUrl": "https://example.com/very/long/url",
  "ShortUrl": "http://localhost:5000/abc123",
  "CreatedAt": "2025-11-26T10:00:00Z",
  "AccessCount": 0
}
```

#### Get User's URLs
```http
GET /api/urls
Authorization: Bearer {token}

Response:
[
  {
    "ShortCode": "abc123",
    "OriginalUrl": "https://example.com",
    "AccessCount": 42,
    "CreatedAt": "2025-11-26T10:00:00Z"
  }
]
```

#### Redirect to Original URL
```http
GET /{shortCode}

Response: 302 Redirect to original URL
```

### Admin Endpoints

#### Get All URLs
```http
GET /api/admin/urls
Authorization: Bearer {admin-token}

Response:
{
  "TotalCount": 156,
  "Urls": [...]
}
```

#### Get All Users
```http
GET /api/admin/users
Authorization: Bearer {admin-token}

Response:
{
  "TotalCount": 23,
  "Users": [...]
}
```

#### Get Statistics
```http
GET /api/admin/stats
Authorization: Bearer {admin-token}

Response:
{
  "TotalUrls": 156,
  "TotalUsers": 23,
  "TotalClicks": 1247,
  "UrlsToday": 12
}
```

---

## 🔐 Authentication

### JWT Token Structure

```json
{
  "sub": "user@example.com",
  "email": "user@example.com",
  "userId": "123",
  "fullName": "John Doe",
  "role": "User",
  "exp": 1735810621,
  "iss": "ShortenerApp",
  "aud": "ShortenerApp.Client"
}
```

### Token Expiration

- **User and Admin Tokens:** 24 hours
- **Refresh:** Login again when expired

### Role-Based Authorization

- **User Role:** Create URLs, view own URLs
- **Admin Role:** Full access, manage users, view all URLs

---

## 🎯 Admin Dashboard

### Features

**📊 Overview Statistics**
- Total URLs created
- Total registered users
- Total clicks tracked
- URLs created today

**🔗 URL Management**
- View all shortened URLs
- Search and filter URLs
- Delete any URL
- View click statistics per URL

**👥 User Management**
- View all registered users
- Search users by email
- Delete users (with safety checks)
- View user registration date

**📈 Analytics**
- Click trends over time
- Most popular URLs
- User activity statistics
- System health metrics

---

## 🌐 Deployment status

The Railway application and API health-check URLs listed in earlier versions returned HTTP 404 when checked on 2026-09-27. No live deployment is currently verified; use the local Docker Compose instructions above.

The current [GitHub Actions workflow](.github/workflows/ci-cd.yml) restores and builds `ShortenerService` and `ApiGateway`. On pushes to `main` or `master`, it is configured to publish `ShortenerService`, `ApiGateway` and the frontend images to GHCR. It does not run a test suite, build or publish the `UserService` image, or deploy to Railway.

---

## 👥 Team

<div align="center">

**AMD201 - Advanced Microservices Development & Deployment**

| Role | Responsibilities |
|------|-----------------|
| 👨‍💼 **Team Leader** | Architecture design, deployment, coordination |
| 👨‍💻 **Backend Developers** | Microservices development, API design |
| 🎨 **Frontend Developer** | Vue.js UI, user experience |
| 🔧 **DevOps Engineer** | Docker, CI/CD, Railway deployment |

</div>

---

## 📄 License

This project is developed for educational purposes as part of the AMD201 course.

---

## 🙏 Acknowledgments

- ASP.NET Core Team for excellent framework
- Ocelot contributors for API Gateway
- Railway for hosting platform
- Docker for containerization technology

---

<div align="center">

**Made with ❤️ by AMD201 Team**

[![GitHub](https://img.shields.io/badge/GitHub-ducanhnguyen223-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/ducanhnguyen223)
[![Railway](https://img.shields.io/badge/Deployed_on-Railway-0B0D0E?style=for-the-badge&logo=railway&logoColor=white)](https://railway.app)

</div>
