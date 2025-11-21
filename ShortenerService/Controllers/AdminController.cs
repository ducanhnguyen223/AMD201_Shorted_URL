using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShortenerService.Data;
using RabbitMQ.Client;
using System.Text.Json;

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

        // GET /api/admin/stats - Thống kê hệ thống
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            // Check if user is admin
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "Admin")
            {
                return Forbid();
            }

            var totalUrls = await _context.ShortenedUrls.CountAsync();
            var totalClicks = await _context.ShortenedUrls.SumAsync(u => u.AccessCount);
            var todayUrls = await _context.ShortenedUrls
                .Where(u => u.CreatedAt.Date == DateTime.UtcNow.Date)
                .CountAsync();

            // Get unique user count
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

        // GET /api/admin/urls - Lấy tất cả URLs
        [HttpGet("urls")]
        public async Task<IActionResult> GetAllUrls()
        {
            // Check if user is admin
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "Admin")
            {
                return Forbid();
            }

            var urls = await _context.ShortenedUrls
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.OriginalUrl,
                    u.ShortCode,
                    u.CustomAlias,
                    u.CreatedAt,
                    u.AccessCount,
                    u.UserId,
                    ShortUrl = $"{Request.Scheme}://{Request.Host}/{u.ShortCode}"
                })
                .ToListAsync();

            return Ok(urls);
        }

        // DELETE /api/admin/urls/{id} - Xóa URL bất kỳ
        [HttpDelete("urls/{id}")]
        public async Task<IActionResult> DeleteUrl(long id)
        {
            // Check if user is admin
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "Admin")
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
    }
}
