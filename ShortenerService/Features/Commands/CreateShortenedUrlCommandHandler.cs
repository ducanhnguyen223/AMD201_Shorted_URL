using MediatR;
using ShortenerService.Data;
using ShortenerService.Entities;
using ShortenerService.Services;
using ShortenerService.Messaging; // Add this using
using System.Threading;
using System.Threading.Tasks;

namespace ShortenerService.Features.Commands
{
    public class CreateShortenedUrlCommandHandler : IRequestHandler<CreateShortenedUrlCommand, ShortenedUrl>
    {
        private readonly ApplicationDbContext _context;
        private readonly IUrlShorteningService _urlShorteningService;
        private readonly IRabbitMQPublisher _publisher; // Add this

        public CreateShortenedUrlCommandHandler(ApplicationDbContext context, IUrlShorteningService urlShorteningService, IRabbitMQPublisher publisher) // Add IRabbitMQPublisher
        {
            _context = context;
            _urlShorteningService = urlShorteningService;
            _publisher = publisher; // Initialize publisher
        }

        public async Task<ShortenedUrl> Handle(CreateShortenedUrlCommand request, CancellationToken cancellationToken)
        {
            var shortenedUrl = new ShortenedUrl
            {
                OriginalUrl = request.OriginalUrl,
                CreatedAt = DateTime.UtcNow,
                AccessCount = 0
            };

            // Save the entity first to get a database-generated ID
            _context.ShortenedUrls.Add(shortenedUrl);
            await _context.SaveChangesAsync(cancellationToken);

            // Now generate the short code using the ID
            shortenedUrl.ShortCode = _urlShorteningService.GenerateShortCode(shortenedUrl.Id);

            // Save again to persist the ShortCode
            await _context.SaveChangesAsync(cancellationToken);

            // Publish the LinkCreatedEvent
            var linkCreatedEvent = new LinkCreatedEvent
            {
                Id = shortenedUrl.Id,
                ShortCode = shortenedUrl.ShortCode,
                OriginalUrl = shortenedUrl.OriginalUrl,
                CreatedAt = shortenedUrl.CreatedAt
            };
            _publisher.PublishMessage(linkCreatedEvent, "link_exchange", "link.created"); // Customize exchange and routing key

            return shortenedUrl;
        }
    }
}
