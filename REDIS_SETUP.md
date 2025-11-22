# Hướng dẫn cấu hình Redis cho ShortenerService trên Railway

## Bước 1: Lấy Redis Private URL

1. Vào Railway Dashboard: https://railway.app/dashboard
2. Chọn project của bạn
3. Click vào **Redis service** (biểu tượng Redis màu đỏ)
4. Vào tab **Variables**
5. Tìm biến tên **REDIS_PRIVATE_URL**
6. Click vào biến đó và copy giá trị (sẽ có dạng như sau):

```
redis://default:xxxxxxxxxxxxx@redis.railway.internal:6379
```

## Bước 2: Thêm vào ShortenerService

### Cách 1: Sử dụng Reference (Khuyến nghị)

1. Vào **ShortenerService** → **Variables**
2. Tìm biến `ConnectionStrings__RedisConnection`
3. Thay vì paste URL trực tiếp, dùng **reference**:

```
${{Redis.REDIS_PRIVATE_URL}}
```

Hoặc nếu biến là REDIS_URL:
```
${{Redis.REDIS_URL}}
```

### Cách 2: Paste trực tiếp

1. Vào **ShortenerService** → **Variables**
2. Tìm biến `ConnectionStrings__RedisConnection`
3. Paste Redis URL vào:

```
redis://default:<password>@redis.railway.internal:6379
```

## Bước 3: Kiểm tra

Sau khi thêm biến:

1. ShortenerService sẽ tự động redeploy
2. Đợi 1-2 phút
3. Test health check:

```bash
curl https://api-gateway-production-e75a.up.railway.app/api/health
```

Kết quả mong đợi:
```json
{
  "status": "healthy",
  "timestamp": "...",
  "database": "connected",
  "redis": "connected"  ← Phải là "connected"
}
```

## Lưu ý quan trọng:

### ✅ Dùng PRIVATE URL (internal)
```
redis://default:xxx@redis.railway.internal:6379
```

### ❌ KHÔNG dùng PUBLIC URL
```
redis://default:xxx@monorail.proxy.rlwy.net:12345
```

PUBLIC URL chỉ dùng khi kết nối từ bên ngoài Railway.

## Nếu không tìm thấy REDIS_PRIVATE_URL:

Có thể Redis service có tên biến khác. Kiểm tra các biến sau trong Redis service:

- `REDIS_URL`
- `REDIS_PRIVATE_URL`
- `REDIS_HOST` + `REDIS_PASSWORD` + `REDIS_PORT`

Nếu có riêng lẻ, tự tạo connection string:
```
redis://default:${REDIS_PASSWORD}@${REDIS_HOST}:${REDIS_PORT}
```

## Troubleshooting:

### Lỗi "Authentication failed"
- Kiểm tra password có đúng không
- Format: `redis://default:<password>@host:port`
- Password phải là chính xác từ biến REDIS_PASSWORD

### Lỗi "Connection refused"
- Dùng PRIVATE URL, không phải PUBLIC URL
- Host phải là `redis.railway.internal` hoặc internal hostname

### Redis vẫn "disconnected"
- Kiểm tra Redis service có đang chạy không (Active)
- Restart Redis service
- Redeploy ShortenerService sau khi cập nhật biến
