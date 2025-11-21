# Deployment Documentation

## Production URLs

### Public URLs
- **Frontend (Vue.js)**: https://amd201shortedurl-production.up.railway.app
- **API Gateway (Ocelot)**: https://api-gateway-production-e75a.up.railway.app
- **Shortener Service**: https://shortenner-service-production.up.railway.app

### Services Architecture
```
┌─────────────────────────────────────────────────────────────┐
│                         RAILWAY CLOUD                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐     ┌───────────────┐    ┌──────────────┐│
│  │   Frontend   │────▶│  API Gateway  │───▶│  Shortener   ││
│  │   (Vue.js)   │     │   (Ocelot)    │    │   Service    ││
│  └──────────────┘     └───────────────┘    └──────┬───────┘│
│                                                     │        │
│                       ┌─────────────────────────────┘        │
│                       │                                      │
│         ┌─────────────┼─────────────┬──────────────┐        │
│         ▼             ▼             ▼              ▼        │
│   ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│   │PostgreSQL│  │  Redis   │  │ RabbitMQ │  │   User   │  │
│   │ Database │  │  Cache   │  │(CloudAMQP)│ │ Service  │  │
│   └──────────┘  └──────────┘  └──────────┘  └────┬─────┘  │
│                                                    │         │
│                                               (Pending)      │
└─────────────────────────────────────────────────────────────┘
```

## Infrastructure Services

### PostgreSQL Database
- **Platform**: Railway
- **Connection**: Internal private network
- **Auto-migration**: Enabled on startup

### Redis Cache
- **Platform**: Railway
- **Purpose**: Distributed caching
- **Connection**: Internal private network

### RabbitMQ Message Broker
- **Platform**: CloudAMQP (Little Lemur - Free tier)
- **Purpose**: Event-driven architecture
- **Connection**: External AMQP URL

## Deployment Process

### 1. Code Changes
```bash
git add .
git commit -m "Your changes"
git push origin master
```

### 2. CI/CD Pipeline (GitHub Actions)
- **Trigger**: Push to master branch
- **Actions**:
  1. Build .NET projects
  2. Build Docker images
  3. Push to GitHub Container Registry (ghcr.io)

### 3. Railway Auto-Deploy
- **Trigger**: Git push or manual redeploy
- **Process**:
  1. Pull latest code from GitHub
  2. Build Docker containers
  3. Apply environment variables
  4. Deploy to production
  5. Health check

## Environment Variables

### ShortenerService
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}};SSL Mode=Require;Trust Server Certificate=true
ConnectionStrings__RedisConnection=${{Redis.REDIS_PRIVATE_URL}}
RabbitMQ__HostName=armadillo.rmq.cloudamqp.com
RabbitMQ__Port=5672
RabbitMQ__UserName=rwrjkqqh
RabbitMQ__Password=<password>
RabbitMQ__VirtualHost=rwrjkqqh
```

### ApiGateway
```
ASPNETCORE_ENVIRONMENT=Production
JwtSettings__Secret=<jwt-secret>
JwtSettings__Issuer=ShortenerApp
JwtSettings__Audience=ShortenerApp.Client
```

### Frontend
```
NODE_ENV=production
VITE_API_GATEWAY_URL=https://api-gateway-production-e75a.up.railway.app
```

## Testing Production

### Test Anonymous URL Shortening
1. Open: https://amd201shortedurl-production.up.railway.app
2. Enter a URL (e.g., `https://google.com`)
3. Click "Rút gọn ngay"
4. Verify shortened URL is generated
5. Click shortened URL to verify redirect

### Test API Directly
```bash
# Shorten URL
curl -X POST https://api-gateway-production-e75a.up.railway.app/api/shorten \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://example.com"}'

# Expected response:
# {
#   "shortUrl": "https://api-gateway-production-e75a.up.railway.app/<code>",
#   "shortCode": "<code>"
# }
```

## Monitoring

### View Logs
1. Go to Railway Dashboard
2. Select service (shortener-service, api-gateway, or frontend)
3. Click **Deployments** tab
4. Click latest deployment
5. View real-time logs

### Check Service Health
- ShortenerService: https://shortenner-service-production.up.railway.app
- API Gateway: https://api-gateway-production-e75a.up.railway.app

## Rollback Process

### Rollback to Previous Deployment
1. Go to Railway Dashboard
2. Select service
3. Click **Deployments** tab
4. Find previous successful deployment
5. Click "Redeploy"

### Rollback Code
```bash
git revert <commit-hash>
git push origin master
```

## Known Issues & Solutions

### Issue 1: RabbitMQ Connection Failed
**Error**: `NOT_ALLOWED - vhost not found`

**Solution**: Verify RabbitMQ environment variables:
- `RabbitMQ__VirtualHost` must be just `rwrjkqqh`, not full URL
- Check CloudAMQP credentials are correct

### Issue 2: Database Connection Failed
**Error**: `Failed to connect to PostgreSQL`

**Solution**:
- Check `ConnectionStrings__DefaultConnection` format
- Ensure using `${{Postgres.PGHOST}}` Railway variable references
- Verify SSL Mode is set correctly

### Issue 3: CORS Error on Frontend
**Error**: `CORS policy blocked`

**Solution**:
- Verify ApiGateway CORS is configured to allow frontend domain
- Check `ocelot.Production.json` BaseUrl matches actual domain

## Future Improvements

### When User Service is Ready
1. Deploy User Service to Railway
2. Update `ocelot.Production.json` with auth routes:
   ```json
   {
     "UpstreamPathTemplate": "/api/auth/{everything}",
     "DownstreamPathTemplate": "/api/auth/{everything}",
     "DownstreamScheme": "https",
     "DownstreamHostAndPorts": [
       {
         "Host": "user-service-production.up.railway.app",
         "Port": 443
       }
     ]
   }
   ```
3. Enable authentication in frontend (LoginView, RegisterView)
4. Test full authentication flow

### Scaling Considerations
- **PostgreSQL**: Upgrade to paid plan for production workload
- **Redis**: Consider Redis Labs for dedicated instance
- **Railway**: Monitor usage and upgrade plan if needed
- **CDN**: Consider Cloudflare for frontend static assets

## Support

### Railway Dashboard
https://railway.app/dashboard

### CloudAMQP Dashboard
https://customer.cloudamqp.com/instance

### GitHub Repository
https://github.com/ducanhnguyen223/AMD201_Shorted_URL

---
**Last Updated**: 2025-11-21
**Deployed By**: DucAnh (Team Leader)
**Week**: 4 - Cloud Deployment
