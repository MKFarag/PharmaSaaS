namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface ICacheService
{
    /// <summary>Gets or creates a cached value using the provided factory function</summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>Sets a value in the cache</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>Removes a specific key from the cache</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Removes a specific key from the caches</summary>
    Task RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>Removes all cache entries with the specified tag</summary>
    Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>Removes all cache entries with the specified tags</summary>
    Task RemoveByTagAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
}
