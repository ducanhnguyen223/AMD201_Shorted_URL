using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShortenerService.Features.Queries;

namespace ShortenerService.Controllers
{
    [ApiController]
    [Route("")]
    public class RedirectController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RedirectController(IMediator mediator)
        {
            _mediator = mediator;
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

                // In a real application, you might want to increment AccessCount here and save to DB.
                // For now, just redirect.
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
