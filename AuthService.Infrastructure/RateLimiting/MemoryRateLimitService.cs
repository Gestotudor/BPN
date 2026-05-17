using AuthService.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AuthService.Infrastructure.RateLimiting;

public sealed class MemoryRateLimitService : IRateLimitService
{
    private const int PermitLimit = 100;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    private static readonly object SyncRoot = new();

    private readonly IMemoryCache _memoryCache;

    public MemoryRateLimitService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<bool> IsRequestAllowedAsync(string apiKeyIdentifier, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var entry = _memoryCache.GetOrCreate(apiKeyIdentifier, cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = Window;
                return new RateLimitEntry(DateTime.UtcNow, 0);
            })!;

            if (DateTime.UtcNow - entry.WindowStartedAt >= Window)
            {
                entry = new RateLimitEntry(DateTime.UtcNow, 0);
            }

            if (entry.RequestCount >= PermitLimit)
            {
                _memoryCache.Set(apiKeyIdentifier, entry, Window);
                return Task.FromResult(false);
            }

            entry = entry with { RequestCount = entry.RequestCount + 1 };
            _memoryCache.Set(apiKeyIdentifier, entry, Window);

            return Task.FromResult(true);
        }
    }

    private sealed record RateLimitEntry(DateTime WindowStartedAt, int RequestCount);
}
