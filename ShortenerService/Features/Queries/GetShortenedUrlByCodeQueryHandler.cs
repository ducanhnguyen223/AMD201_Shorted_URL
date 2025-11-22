using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ShortenerService.Data;
using ShortenerService.Entities;

namespace ShortenerService.Features.Queries
{
    public class GetShortenedUrlByCodeQueryHandlers
        : IRequestHandler<GetShortenedUrlByCodeQuery, ShortenedUrl?>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public GetShortenedUrlByCodeQueryHandlers(
            ApplicationDbContext context,
            IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ShortenedUrl?> Handle(
            GetShortenedUrlByCodeQuery request,
            CancellationToken cancellationToken)
        {
            string cacheKey = $"shortcode-{request.ShortCode}";

            // 1. Try to get from cache
            string? cachedUrlJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedUrlJson))
            {
                return JsonSerializer.Deserialize<ShortenedUrl?>(cachedUrlJson);
            }

            // 2. Cache miss: Get from database
            var shortenedUrl = await _context.ShortenedUrls
                .FirstOrDefaultAsync(
                    s => s.ShortCode == request.ShortCode,
                    cancellationToken);

            if (shortenedUrl != null)
            {
                // 3. Save to cache for next time
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));

                var urlJson = JsonSerializer.Serialize(shortenedUrl);
                await _cache.SetStringAsync(cacheKey, urlJson, options, cancellationToken);
            }

            return shortenedUrl;
        }
    }
}
