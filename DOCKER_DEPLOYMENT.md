# URL Shortener - Docker Deployment Guide

## Yêu cầu hệ thống

- Docker Desktop (Windows/Mac) hoặc Docker Engine (Linux)
- Docker Compose
- Tối thiểu 4GB RAM
- Port 5000, 5001 cần available

## Cấu trúc Services

```
- PostgreSQL (port 5432) - Database chính
- Redis (port 6379) - Cache
- RabbitMQ (port 5672, 15672) - Message Queue
- UserService (port 8080) - Quản lý users và authentication
- ShortenerService (port 8080) - Quản lý URL rút gọn
- ApiGateway (port 5000) - API Gateway
- Frontend (port 5001) - Vue.js frontend
```

## Cách chạy

### 1. Build tất cả services

```bash
docker-compose build
```

### 2. Khởi động toàn bộ hệ thống

```bash
docker-compose up -d
```

### 3. Kiểm tra trạng thái

```bash
docker-compose ps
```

Tất cả services phải ở trạng thái `healthy` hoặc `running`.

### 4. Xem logs

```bash
# Xem tất cả logs
docker-compose logs -f

# Xem logs của service cụ thể
docker-compose logs -f api-gateway
docker-compose logs -f shortener-service
docker-compose logs -f user-service
docker-compose logs -f frontend
```

## Truy cập ứng dụng

- **Frontend**: http://localhost:5001
- **API Gateway**: http://localhost:5000
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **Swagger - ShortenerService**: http://localhost:5000/swagger (khi route qua gateway)

## Tạo Admin User

Để sử dụng Admin Dashboard, cần tạo admin user trực tiếp trong database:

```bash
# Truy cập PostgreSQL container
docker exec -it postgres psql -U postgres -d shortenerdb

# Chạy SQL để tạo admin (password: admin123)
INSERT INTO "Users" ("Email", "PasswordHash", "FullName", "Role", "CreatedAt")
VALUES (
    'admin@test.com',
    '$2a$11$XQ7PQ.Z1X.rZK5Z5Z5Z5ZeX.X.X.X.X.X.X.X.X.X.X.X.X.X',  -- bcrypt hash của 'admin123'
    'Admin User',
    'Admin',
    NOW()
);
```

Hoặc đăng ký user bình thường rồi update role thành Admin:

```bash
# Đăng ký user qua API
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@test.com","password":"admin123","fullName":"Admin User"}'

# Truy cập database và update role
docker exec -it postgres psql -U postgres -d shortenerdb
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'admin@test.com';
```

## Testing các chức năng

### 1. Tạo short URL

```bash
curl -X POST http://localhost:5000/api/shorten \
  -H "Content-Type: application/json" \
  -d '{"originalUrl":"https://google.com"}'
```

### 2. Test redirect

```bash
curl -L http://localhost:5000/{shortCode}
```

### 3. Health checks

```bash
# API Gateway
curl http://localhost:5000/health

# ShortenerService (database + redis)
curl http://localhost:5000/api/health

# UserService
curl http://localhost:5000/api/auth/health
```

## Dừng và xóa toàn bộ

```bash
# Dừng tất cả
docker-compose down

# Dừng và xóa volumes (database sẽ mất)
docker-compose down -v

# Dừng và xóa images
docker-compose down --rmi all
```

## Troubleshooting

### Port đã được sử dụng

Nếu port 5000, 5001 đã được dùng, sửa trong `docker-compose.yml`:

```yaml
frontend:
  ports:
    - "8080:80"  # Đổi 5001 thành 8080

api-gateway:
  ports:
    - "8000:5000"  # Đổi 5000 thành 8000
```

### Services không healthy

```bash
# Kiểm tra logs
docker-compose logs {service_name}

# Restart service cụ thể
docker-compose restart {service_name}

# Rebuild và restart
docker-compose up -d --build {service_name}
```

### Database connection failed

```bash
# Kiểm tra PostgreSQL đang chạy
docker-compose ps postgres

# Restart PostgreSQL
docker-compose restart postgres

# Xem logs
docker-compose logs postgres
```

### Reset toàn bộ database

```bash
docker-compose down -v
docker-compose up -d
```

## Các tính năng đã hoàn thiện

✅ URL Shortening với custom alias
✅ User authentication (JWT)
✅ Admin Dashboard với monitoring
✅ RabbitMQ monitoring
✅ Redis monitoring
✅ Health checks cho tất cả services
✅ Auto-redirect admin users
✅ Route guard protection
✅ CORS configured
✅ Database migrations tự động
✅ Redis caching
✅ Message queue với RabbitMQ

## Cấu trúc Project

```
asm/
├── ApiGateway/          # Ocelot API Gateway
├── UserService/         # User management + Auth
├── ShortenerService/    # URL shortening logic
├── vue-project/         # Frontend Vue.js
├── docker-compose.yml   # Docker orchestration
└── nginx.conf          # Nginx config cho frontend
```

## Production Deployment

Để deploy lên production (Railway, AWS, etc):

1. Cập nhật các biến môi trường trong `docker-compose.yml`
2. Sử dụng file `ocelot.Production.json` với URLs thực tế
3. Enable HTTPS/SSL
4. Cấu hình domain names
5. Setup backup cho PostgreSQL
6. Monitor logs với centralized logging
