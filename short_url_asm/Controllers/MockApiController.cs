using Microsoft.AspNetCore.Mvc;
using short_url_asm.DTOs;

namespace short_url_asm.Controllers
{
    [ApiController]
    [Route("api/mock")]
    public class MockApiController : ControllerBase
    {
        // --- Auth Service Mocks (for Đ.A) ---

        [HttpPost("register")]
        public AuthResponseDto Register([FromBody] RegisterRequestDto request)
        {
            return new AuthResponseDto();
        }

        [HttpPost("login")]
        public AuthResponseDto Login([FromBody] LoginRequestDto request)
        {
            return new AuthResponseDto();
        }

        // --- CRUD Link Mocks (for Tiến) ---

        [HttpPost("urls/shorten")]
        public UrlResponseDto CreateShortUrl([FromBody] CreateShortUrlRequestDto request)
        {
            return new UrlResponseDto();
        }

        [HttpGet("my-links")]
        public IEnumerable<UrlResponseDto> GetMyLinks()
        {
            return new List<UrlResponseDto>();
        }

        [HttpDelete("link/{id}")]
        public IActionResult DeleteLink(string id)
        {
            return NoContent();
        }

        // --- Profile Mocks (for Tiến) ---
        [HttpGet("profile")]
        public object GetProfile()
        {
            return new { };
        }

        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] object profile)
        {
            return Ok();
        }
    }
}
