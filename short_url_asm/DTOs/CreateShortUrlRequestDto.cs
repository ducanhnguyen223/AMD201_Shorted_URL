using System.ComponentModel.DataAnnotations;

namespace short_url_asm.DTOs
{
    public class CreateShortUrlRequestDto
    {
        [Required]
        [Url]
        public string OriginalUrl { get; set; }
    }
}
