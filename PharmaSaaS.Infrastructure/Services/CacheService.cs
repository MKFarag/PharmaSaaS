using Microsoft.Extensions.Caching.Hybrid;

namespace PharmaSaaS.Infrastructure.Services;

public class CacheService(HybridCache cache) : ICacheService
{
    private const double _localCacheRatio = 0.2;

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        var options = expiration.HasValue
            ? new HybridCacheEntryOptions
            {
                Expiration = expiration.Value,
                LocalCacheExpiration = expiration.Value * _localCacheRatio
            }
            : null;

        return await cache.GetOrCreateAsync(
            key,
            async token => await factory(token),
            options,
            tags,
            cancellationToken);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        var options = expiration.HasValue
            ? new HybridCacheEntryOptions
            {
                Expiration = expiration.Value,
                LocalCacheExpiration = expiration.Value
            }
            : null;

        await cache.SetAsync(key, value, options, tags, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => await cache.RemoveAsync(key, cancellationToken);

    public async Task RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        => await cache.RemoveAsync(keys, cancellationToken);

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        => await cache.RemoveByTagAsync(tag, cancellationToken);

    public async Task RemoveByTagAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
        => await cache.RemoveByTagAsync(tags, cancellationToken);
}
