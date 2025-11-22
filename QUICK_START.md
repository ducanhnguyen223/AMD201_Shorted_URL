# URL Shortener - Hướng dẫn nhanh

## Khởi động hệ thống

### Cách 1: Sử dụng script tự động (Windows)

```bash
docker-manager.bat
```

Chọn tùy chọn phù hợp từ menu.

### Cách 2: Sử dụng lệnh trực tiếp

```bash
# Build tất cả
docker-compose build

# Khởi động
docker-compose up -d

# Kiểm tra trạng thái
docker-compose ps
```

## Truy cập ứng dụng

- **Trang chủ**: http://localhost:3000
- **API**: http://localhost:5000
- **RabbitMQ**: http://localhost:15672 (guest/guest)

## Tạo tài khoản Admin

1. Truy cập http://localhost:3000
2. Click "Đăng ký"
3. Nhập thông tin:
   - Email: admin@test.com
   - Password: admin123
   - Họ tên: Admin User

4. Sau khi đăng ký, chạy lệnh sau để nâng cấp lên Admin:

```bash
docker exec -it postgres psql -U postgres -d shortenerdb -c "UPDATE \"Users\" SET \"Role\" = 'Admin' WHERE \"Email\" = 'admin@test.com';"
```

5. Đăng xuất và đăng nhập lại
6. Hệ thống sẽ tự động chuyển đến Admin Dashboard

## Tính năng chính

### Người dùng thường
- ✅ Rút gọn URL (với hoặc không custom alias)
- ✅ Xem danh sách URL đã tạo
- ✅ Chỉnh sửa URL gốc
- ✅ Xóa URL
- ✅ Xem số lượt click

### Admin
- ✅ Tất cả tính năng của user thường
- ✅ Xem tất cả URL trong hệ thống
- ✅ Xem danh sách tất cả users
- ✅ Xóa URL/User bất kỳ
- ✅ Xem thống kê hệ thống
- ✅ Monitor RabbitMQ
- ✅ Monitor Redis
- ✅ Health checks tất cả services

## Test nhanh

### 1. Tạo URL rút gọn

```bash
curl -X POST http://localhost:5000/api/shorten \
  -H "Content-Type: application/json" \
  -d '{"originalUrl":"https://google.com"}'
```

Response:
```json
{
  "originalUrl": "https://google.com",
  "shortUrl": "http://localhost:5000/abc123",
  "shortCode": "abc123",
  "customAlias": null
}
```

### 2. Test redirect

```bash
curl -L http://localhost:5000/abc123
```

Sẽ redirect đến Google.

### 3. Kiểm tra health

```bash
curl http://localhost:5000/api/health
```

Response:
```json
{
  "status": "healthy",
  "timestamp": "2025-11-22T12:00:00Z",
  "database": "connected",
  "redis": "connected"
}
```

## Dừng hệ thống

```bash
docker-compose down
```

## Xóa toàn bộ (bao gồm database)

```bash
docker-compose down -v
```

## Xem logs

```bash
# Tất cả services
docker-compose logs -f

# Service cụ thể
docker-compose logs -f api-gateway
docker-compose logs -f shortener-service
docker-compose logs -f user-service
```

## Troubleshooting

### Lỗi "port already in use"

Dừng service đang dùng port hoặc đổi port trong `docker-compose.yml`:

```yaml
api-gateway:
  ports:
    - "8000:5000"  # Đổi từ 5000 thành 8000
```

### Services không khởi động

```bash
# Xem logs để tìm lỗi
docker-compose logs {service_name}

# Rebuild
docker-compose up -d --build {service_name}
```

### Reset database

```bash
docker-compose down -v
docker-compose up -d
```

## Cấu trúc dự án

```
asm/
├── ApiGateway/              # API Gateway (Ocelot)
├── UserService/             # User & Auth service
├── ShortenerService/        # URL shortening service
├── vue-project/             # Frontend (Vue.js)
├── docker-compose.yml       # Docker orchestration
├── docker-manager.bat       # Management script (Windows)
├── DOCKER_DEPLOYMENT.md     # Hướng dẫn chi tiết
└── QUICK_START.md          # File này
```

## Production Notes

- Thay đổi JWT Secret trong `appsettings.json`
- Cấu hình HTTPS/SSL
- Sử dụng PostgreSQL managed service
- Setup Redis cluster
- Enable monitoring và logging
- Backup database định kỳ
