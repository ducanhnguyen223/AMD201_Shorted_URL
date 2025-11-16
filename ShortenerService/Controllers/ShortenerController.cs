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
    }
}
