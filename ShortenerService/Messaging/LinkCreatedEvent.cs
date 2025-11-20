namespace ShortenerService.Messaging
{
    public class LinkCreatedEvent
    {
        public long Id { get; set; }
        public string ShortCode { get; set; }
        public string OriginalUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}