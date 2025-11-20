namespace short_url_asm.DTOs
{
    public class UrlResponseDto
    {
        public long Id { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccessCount { get; set; }
    }
}
