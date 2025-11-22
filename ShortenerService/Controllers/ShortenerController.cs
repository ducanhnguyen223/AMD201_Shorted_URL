using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShortenerService.Data;
using ShortenerService.Features.Commands;
using ShortenerService.DTOs;

namespace ShortenerService.Controllers
{
    [ApiController]
    [Route("api")]
    public class ShortenerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;

        public ShortenerController(IMediator mediator, ApplicationDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> CreateShortenedUrl([FromBody] CreateShortUrlRequest request)
        {
            // Extract UserId from JWT token if authenticated
            int? userId = null;
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            var command = new CreateShortenedUrlCommand
            {
                OriginalUrl = request.OriginalUrl,
                CustomAlias = request.CustomAlias,
                UserId = userId
            };

            try
            {
                var result = await _mediator.Send(command);

                // Use gateway URL from configuration or request headers
                var gatewayUrl = Request.Headers["X-Forwarded-Host"].FirstOrDefault()
                    ?? Request.Headers["X-Original-Host"].FirstOrDefault()
                    ?? Request.Host.Value;

                var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault()
                    ?? Request.Scheme;

                var response = new
                {
                    OriginalUrl = result.OriginalUrl,
                    ShortUrl = $"{scheme}://{gatewayUrl}/{result.ShortCode}",
                    ShortCode = result.ShortCode,
                    CustomAlias = result.CustomAlias
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // User Dashboard endpoints
        [HttpGet("urls")]
        [Authorize]
        public async Task<IActionResult> GetMyUrls()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var urls = await _context.ShortenedUrls
                .Where(u => u.UserId == userId)
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
                ShortUrl = $"{scheme}://{gatewayUrl}/{u.ShortCode}"
            });

            return Ok(result);
        }

        [HttpPut("urls/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUrl(long id, [FromBody] UpdateUrlRequest request)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var url = await _context.ShortenedUrls.FindAsync(id);
            if (url == null)
            {
                return NotFound();
            }

            if (url.UserId != userId)
            {
                return Forbid();
            }

            url.OriginalUrl = request.OriginalUrl;
            await _context.SaveChangesAsync();

            // Use gateway URL from configuration or request headers
            var gatewayUrl = Request.Headers["X-Forwarded-Host"].FirstOrDefault()
                ?? Request.Headers["X-Original-Host"].FirstOrDefault()
                ?? Request.Host.Value;

            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault()
                ?? Request.Scheme;

            return Ok(new
            {
                url.Id,
                url.OriginalUrl,
                url.ShortCode,
                url.CustomAlias,
                url.CreatedAt,
                url.AccessCount,
                ShortUrl = $"{scheme}://{gatewayUrl}/{url.ShortCode}"
            });
        }

        [HttpDelete("urls/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUrl(long id)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var url = await _context.ShortenedUrls.FindAsync(id);
            if (url == null)
            {
                return NotFound();
            }

            if (url.UserId != userId)
            {
                return Forbid();
            }

            _context.ShortenedUrls.Remove(url);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class UpdateUrlRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
    }
}
