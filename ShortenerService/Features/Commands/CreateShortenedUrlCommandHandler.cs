using MediatR;
using ShortenerService.Data;
using ShortenerService.Entities;
using ShortenerService.Services;
using System.Threading;
using System.Threading.Tasks;

namespace ShortenerService.Features.Commands
{
    public class CreateShortenedUrlCommandHandler : IRequestHandler<CreateShortenedUrlCommand, ShortenedUrl>
    {
        private readonly ApplicationDbContext _context;
        private readonly IUrlShorteningService _urlShorteningService;

        public CreateShortenedUrlCommandHandler(ApplicationDbContext context, IUrlShorteningService urlShorteningService)
        {
            _context = context;
            _urlShorteningService = urlShorteningService;
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

            return shortenedUrl;
        }
    }
}
