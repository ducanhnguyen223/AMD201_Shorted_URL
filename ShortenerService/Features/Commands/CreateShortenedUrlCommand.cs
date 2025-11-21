using MediatR;
using ShortenerService.Entities;

namespace ShortenerService.Features.Commands
{
    public class CreateShortenedUrlCommand : IRequest<ShortenedUrl>
    {
        public string OriginalUrl { get; set; }
        public string? CustomAlias { get; set; }
        public int? UserId { get; set; }
    }
}