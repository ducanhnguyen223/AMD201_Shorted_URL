using MediatR;
using Microsoft.EntityFrameworkCore;
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
            // Check if custom alias already exists
            if (!string.IsNullOrEmpty(request.CustomAlias))
            {
                var existingAlias = await _context.ShortenedUrls
                    .FirstOrDefaultAsync(u => u.CustomAlias == request.CustomAlias, cancellationToken);
                if (existingAlias != null)
                {
                    throw new InvalidOperationException($"Custom alias '{request.CustomAlias}' is already taken");
                }
            }

            var shortenedUrl = new ShortenedUrl
            {
                OriginalUrl = request.OriginalUrl,
                CreatedAt = DateTime.UtcNow,
                AccessCount = 0,
                UserId = request.UserId,
                CustomAlias = request.CustomAlias
            };

            // Save the entity first to get a database-generated ID
            _context.ShortenedUrls.Add(shortenedUrl);
            await _context.SaveChangesAsync(cancellationToken);

            // Use custom alias if provided, otherwise generate short code
            if (!string.IsNullOrEmpty(request.CustomAlias))
            {
                shortenedUrl.ShortCode = request.CustomAlias;
            }
            else
            {
                shortenedUrl.ShortCode = _urlShorteningService.GenerateShortCode(shortenedUrl.Id);
            }

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
