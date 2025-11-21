using System.ComponentModel.DataAnnotations;

namespace ShortenerService.DTOs
{
    public class CreateShortUrlRequest
    {
        [Required]
        [Url]
        public string OriginalUrl { get; set; }

        public string? CustomAlias { get; set; } // Optional custom alias for registered users
    }
}
