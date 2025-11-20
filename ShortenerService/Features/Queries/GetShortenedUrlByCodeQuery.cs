using MediatR;
using ShortenerService.Entities;

namespace ShortenerService.Features.Queries
{
    public class GetShortenedUrlByCodeQuery : IRequest<ShortenedUrl>
    {
        public string ShortCode { get; set; } = string.Empty;
    }
}
