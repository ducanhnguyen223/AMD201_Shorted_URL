using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShortenerService.Features.Commands;
using ShortenerService.DTOs;

namespace ShortenerService.Controllers
{
    [ApiController]
    [Route("api")]
    public class ShortenerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShortenerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> CreateShortenedUrl([FromBody] CreateShortUrlRequest request)
        {
            var command = new CreateShortenedUrlCommand { OriginalUrl = request.OriginalUrl };
            var result = await _mediator.Send(command);

            var response = new 
            {
                OriginalUrl = result.OriginalUrl,
                ShortUrl = $"{Request.Scheme}://{Request.Host}/{result.ShortCode}",
                ShortCode = result.ShortCode
            };

            return Ok(response);
        }

        [HttpGet("/{shortCode}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            var query = new Features.Queries.GetShortenedUrlByCodeQuery { ShortCode = shortCode };
            var result = await _mediator.Send(query);

            if (result == null || string.IsNullOrEmpty(result.OriginalUrl))
            {
                return NotFound();
            }

            // In a real application, you might want to increment AccessCount here and save to DB.
            // For now, just redirect.
            return Redirect(result.OriginalUrl);
        }
    }
}
