using System.ComponentModel.DataAnnotations;

namespace ShortenerService.Entities
{
    public class ShortenedUrl
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string OriginalUrl { get; set; } = string.Empty;
        [Required]
        public string ShortCode { get; set; } = string.Empty ;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public int AccessCount { get; set; } = 0;
    }
}