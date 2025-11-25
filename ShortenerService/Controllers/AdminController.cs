using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ShortenerService.Data;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Text.Json;
using System.Security.Claims;

namespace ShortenerService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Helper method to check admin role
        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            return roleClaim?.Value == "Admin";
        }

        // GET /api/admin/stats - Thống kê hệ thống
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                if (!IsAdmin())
                {
                    return Forbid();
                }

                var totalUrls = await _context.ShortenedUrls.CountAsync();
                var totalClicks = totalUrls > 0
                    ? await _context.ShortenedUrls.SumAsync(u => u.AccessCount)
                    : 0;

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);
                var todayUrls = await _context.ShortenedUrls
                    .Where(u => u.CreatedAt >= today && u.CreatedAt < tomorrow)
                    .CountAsync();

                // Get unique user count from URLs created by users
                var totalUsers = await _context.ShortenedUrls
                    .Where(u => u.UserId != null)
                    .Select(u => u.UserId)
                    .Distinct()
                    .CountAsync();

                return Ok(new
                {
                    totalUrls,
                    totalUsers,
                    totalClicks,
                    todayUrls
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStats: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET /api/admin/urls - Lấy tất cả URLs
        [HttpGet("urls")]
        public async Task<IActionResult> GetAllUrls()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var urls = await _context.ShortenedUrls
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // Use gateway URL from configuration or request headers
            var gatewayUrl = Request.Headers["X-Forwarded-Host"].FirstOrDefault()
                ?? Request.Headers["X-Original-Host"].FirstOrDefault()
                ?? Request.Host.Value;

            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault()
                ?? Request.Scheme;

            var result = urls.Select(u => new
            {
                u.Id,
                u.OriginalUrl,
                u.ShortCode,
                u.CustomAlias,
                u.CreatedAt,
                u.AccessCount,
                u.UserId,
                ShortUrl = $"{scheme}://{gatewayUrl}/{u.ShortCode}"
            });

            return Ok(result);
        }

        // DELETE /api/admin/urls/{id} - Xóa URL bất kỳ
        [HttpDelete("urls/{id}")]
        public async Task<IActionResult> DeleteUrl(long id)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var url = await _context.ShortenedUrls.FindAsync(id);
            if (url == null)
            {
                return NotFound();
            }

            _context.ShortenedUrls.Remove(url);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/admin/rabbitmq/stats - Thống kê RabbitMQ
        [HttpGet("rabbitmq/stats")]
        public IActionResult GetRabbitMQStats()
        {
            // Check if user is admin
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "Admin")
            {
                return Forbid();
            }

            try
            {
                var rabbitMQConfig = _configuration.GetSection("RabbitMQ");
                var hostName = rabbitMQConfig["HostName"] ?? "localhost";
                var port = int.Parse(rabbitMQConfig["Port"] ?? "5672");
                var userName = rabbitMQConfig["UserName"] ?? "guest";
                var password = rabbitMQConfig["Password"] ?? "guest";
                var virtualHost = rabbitMQConfig["VirtualHost"] ?? "/";

                var factory = new ConnectionFactory()
                {
                    HostName = hostName,
                    Port = port,
                    UserName = userName,
                    Password = password,
                    VirtualHost = virtualHost
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Get queue info for known queues
                var queues = new List<object>();
                var knownQueues = new[] { "link_created", "link_exchange" };

                foreach (var queueName in knownQueues)
                {
                    try
                    {
                        var queueDeclare = channel.QueueDeclarePassive(queueName);
                        queues.Add(new
                        {
                            name = queueName,
                            messages = queueDeclare.MessageCount,
                            consumers = queueDeclare.ConsumerCount
                        });
                    }
                    catch
                    {
                        // Queue doesn't exist, skip
                    }
                }

                return Ok(new
                {
                    queues,
                    connections = 1,
                    channels = 1,
                    messages = queues.Sum(q => ((dynamic)q).messages),
                    isConnected = true
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    queues = new List<object>(),
                    connections = 0,
                    channels = 0,
                    messages = 0,
                    isConnected = false,
                    error = ex.Message
                });
            }
        }

        // GET /api/admin/redis/stats - Thống kê Redis
        [HttpGet("redis/stats")]
        public IActionResult GetRedisStats()
        {
            // Check if user is admin
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "Admin")
            {
                return Forbid();
            }

            try
            {
                var redisConnectionString = _configuration.GetConnectionString("Redis") ?? "localhost:6379";
                var connection = ConnectionMultiplexer.Connect(redisConnectionString);
                var server = connection.GetServer(connection.GetEndPoints()[0]);
                var db = connection.GetDatabase();

                var info = server.Info();
                var serverInfo = info.FirstOrDefault(g => g.Key == "Server");
                var clientsInfo = info.FirstOrDefault(g => g.Key == "Clients");
                var memoryInfo = info.FirstOrDefault(g => g.Key == "Memory");
                var statsInfo = info.FirstOrDefault(g => g.Key == "Stats");
                var keyspaceInfo = info.FirstOrDefault(g => g.Key == "Keyspace");

                // Parse uptime
                var uptimeSeconds = serverInfo?.FirstOrDefault(x => x.Key == "uptime_in_seconds").Value ?? "0";
                var uptimeDays = int.Parse(uptimeSeconds) / 86400;
                var uptimeHours = (int.Parse(uptimeSeconds) % 86400) / 3600;
                var uptimeString = uptimeDays > 0 ? $"{uptimeDays} days" : $"{uptimeHours} hours";

                // Parse memory
                var usedMemoryBytes = long.Parse(memoryInfo?.FirstOrDefault(x => x.Key == "used_memory").Value ?? "0");
                var usedMemoryMB = usedMemoryBytes / (1024.0 * 1024.0);
                var memoryString = usedMemoryMB < 1 ? $"{usedMemoryBytes / 1024.0:F1} KB" : $"{usedMemoryMB:F1} MB";

                // Parse clients
                var connectedClients = int.Parse(clientsInfo?.FirstOrDefault(x => x.Key == "connected_clients").Value ?? "0");

                // Parse stats
                var totalCommands = long.Parse(statsInfo?.FirstOrDefault(x => x.Key == "total_commands_processed").Value ?? "0");
                var keyspaceHits = long.Parse(statsInfo?.FirstOrDefault(x => x.Key == "keyspace_hits").Value ?? "0");
                var keyspaceMisses = long.Parse(statsInfo?.FirstOrDefault(x => x.Key == "keyspace_misses").Value ?? "0");
                var opsPerSec = int.Parse(statsInfo?.FirstOrDefault(x => x.Key == "instantaneous_ops_per_sec").Value ?? "0");

                // Calculate hit rate
                var totalKeyOps = keyspaceHits + keyspaceMisses;
                var hitRate = totalKeyOps > 0 ? (keyspaceHits * 100.0 / totalKeyOps) : 0;

                // Get total keys
                var totalKeys = 0;
                if (keyspaceInfo != null)
                {
                    foreach (var ks in keyspaceInfo)
                    {
                        var parts = ks.Value.Split(',');
                        var keysPart = parts.FirstOrDefault(p => p.StartsWith("keys="));
                        if (keysPart != null)
                        {
                            totalKeys += int.Parse(keysPart.Split('=')[1]);
                        }
                    }
                }

                connection.Close();

                return Ok(new
                {
                    isConnected = true,
                    usedMemory = memoryString,
                    totalKeys,
                    connectedClients,
                    uptime = uptimeString,
                    hitRate = $"{hitRate:F1}%",
                    opsPerSec
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    isConnected = false,
                    usedMemory = "N/A",
                    totalKeys = 0,
                    connectedClients = 0,
                    uptime = "N/A",
                    hitRate = "N/A",
                    opsPerSec = 0,
                    error = ex.Message
                });
            }
        }
    }
}
