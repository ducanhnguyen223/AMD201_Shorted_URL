using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShortenerService.Features.Queries;
using ShortenerService.Data;
using Microsoft.EntityFrameworkCore;

namespace ShortenerService.Controllers
{
    [ApiController]
    [Route("")]
    public class RedirectController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;

        public RedirectController(IMediator mediator, ApplicationDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            try
            {
                var query = new GetShortenedUrlByCodeQuery { ShortCode = shortCode };
                var result = await _mediator.Send(query);

                if (result == null || string.IsNullOrEmpty(result.OriginalUrl))
                {
                    return NotFound();
                }

                // Increment access count
                var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
                if (url != null)
                {
                    url.AccessCount++;
                    await _context.SaveChangesAsync();
                }

                return Redirect(result.OriginalUrl);
            }
            catch (Exception ex)
            {
                // Log exception for debugging
                return StatusCode(500, new { error = ex.Message, innerError = ex.InnerException?.Message });
            }
        }
    }
}
