# Week 4 Checklist - Cloud Deployment & CI/CD
**Team Leader: DucAnh | Target: Distinction (9-10 points)**

## ✅ Phase 1: Docker Containerization (COMPLETED)
- [x] Create Dockerfile for ShortenerService
- [x] Create Dockerfile for ApiGateway
- [x] Create Dockerfile for Frontend (Vue.js)
- [x] Create docker-compose.yml with all services
- [x] Configure service dependencies and health checks
- [x] Test all containers locally
- [x] Fix authentication for anonymous URL shortening
- [x] Verify frontend connects to API Gateway correctly
- [x] Document Docker setup in DOCKER_SETUP.md

## 🔄 Phase 2: CI/CD Pipeline Setup (IN PROGRESS)
### GitHub Actions Configuration
- [ ] Initialize Git repository (if not done)
  ```bash
  git init
  git add .
  git commit -m "Initial commit with Docker setup"
  ```
- [ ] Create GitHub repository
- [ ] Push code to GitHub
  ```bash
  git remote add origin <your-repo-url>
  git branch -M main
  git push -u origin main
  ```
- [ ] Create `.github/workflows/ci-cd.yml`
- [ ] Configure workflow to:
  - [ ] Run on push to main branch
  - [ ] Build Docker images
  - [ ] Run tests (if any)
  - [ ] Push images to Docker Hub or GitHub Container Registry
- [ ] Test CI/CD pipeline with a test commit

### Docker Registry Setup
- [ ] Create Docker Hub account (hub.docker.com)
- [ ] Create repositories for:
  - [ ] shortener-service
  - [ ] api-gateway
  - [ ] frontend
- [ ] Add Docker Hub credentials to GitHub Secrets:
  - [ ] `DOCKER_USERNAME`
  - [ ] `DOCKER_PASSWORD`

## ☁️ Phase 3: Cloud Deployment
### Option A: Render (Recommended for free tier)
- [ ] Create Render account (render.com)
- [ ] Deploy infrastructure services:
  - [ ] PostgreSQL database (instead of SQL Server - free tier)
    - Note: May need to switch from SQL Server to PostgreSQL
  - [ ] Redis (via Redis Labs/Upstash)
  - [ ] CloudAMQP for RabbitMQ
- [ ] Deploy application services:
  - [ ] Create Web Service for ShortenerService
  - [ ] Create Web Service for ApiGateway
  - [ ] Create Static Site for Frontend
- [ ] Configure environment variables on Render
- [ ] Update connection strings for cloud services
- [ ] Test deployment

### Option B: Railway (Alternative)
- [ ] Create Railway account (railway.app)
- [ ] Create new project
- [ ] Deploy from GitHub repository
- [ ] Add services:
  - [ ] PostgreSQL
  - [ ] Redis
  - [ ] RabbitMQ
  - [ ] ShortenerService
  - [ ] ApiGateway
  - [ ] Frontend
- [ ] Configure environment variables
- [ ] Test deployment

### Option C: Azure (For full features)
- [ ] Create Azure account (free tier available)
- [ ] Create Resource Group
- [ ] Deploy Azure SQL Database
- [ ] Deploy Azure Cache for Redis
- [ ] Deploy Azure Service Bus (instead of RabbitMQ)
- [ ] Deploy Container Instances or App Services
- [ ] Configure networking and security
- [ ] Test deployment

## 🔧 Phase 4: Production Configuration
- [ ] Create production environment files:
  - [ ] `appsettings.Production.json` for ShortenerService
  - [ ] `appsettings.Production.json` for ApiGateway
  - [ ] `ocelot.Production.json` for ApiGateway
- [ ] Update frontend API URL to production gateway
- [ ] Configure CORS for production domains
- [ ] Set up environment variables for secrets:
  - [ ] Database connection strings
  - [ ] Redis connection string
  - [ ] RabbitMQ credentials
  - [ ] JWT secret key
- [ ] Enable HTTPS/SSL certificates
- [ ] Configure rate limiting (if required)

## 📊 Phase 5: Monitoring & Documentation
- [ ] Set up basic monitoring:
  - [ ] Health check endpoints
  - [ ] Application logs
  - [ ] Error tracking (optional: Sentry)
- [ ] Update documentation:
  - [ ] Add deployment URLs to README
  - [ ] Document environment variables
  - [ ] Create DEPLOYMENT.md guide
  - [ ] Update architecture diagram with cloud services
- [ ] Create demo video showing:
  - [ ] Local Docker deployment
  - [ ] Cloud deployment
  - [ ] CI/CD pipeline in action
  - [ ] Anonymous URL shortening feature

## 🔄 Phase 6: User Service Integration (When Ready)
- [ ] Wait for Tiến and Đ.A to complete User Service
- [ ] Review User Service code
- [ ] Create Dockerfile for User Service
- [ ] Add User Service to docker-compose.yml
- [ ] Update ApiGateway routes for authentication
- [ ] Test login/register functionality
- [ ] Deploy User Service to cloud
- [ ] Enable authentication for analytics features

## 📝 Week 4 Deliverables Checklist
- [ ] Docker Compose file with all services
- [ ] GitHub repository with code
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Cloud deployment (Render/Railway/Azure)
- [ ] Updated architecture diagram
- [ ] DEPLOYMENT.md documentation
- [ ] Demo video (5-10 minutes)
- [ ] Presentation slides for Week 4

## 🎯 Priority Order
1. **HIGH PRIORITY** - Complete CI/CD setup (Phase 2)
2. **HIGH PRIORITY** - Deploy to cloud (Phase 3)
3. **MEDIUM PRIORITY** - Production configuration (Phase 4)
4. **MEDIUM PRIORITY** - Documentation and monitoring (Phase 5)
5. **LOW PRIORITY** - Wait for User Service integration (Phase 6)

## 📌 Important Notes
- Docker containerization is DONE ✅
- All services running locally on Docker
- Anonymous URL shortening works without authentication
- Frontend: http://localhost:3000
- API Gateway: http://localhost:5000
- Next immediate task: Set up GitHub Actions CI/CD

## 🚀 Quick Start Commands
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Rebuild specific service
docker-compose up -d --build <service-name>

# Test API
curl -X POST http://localhost:5000/api/shorten \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://example.com"}'
```

---
**Last Updated:** 2025-11-20
**Status:** Phase 1 Complete, Starting Phase 2
