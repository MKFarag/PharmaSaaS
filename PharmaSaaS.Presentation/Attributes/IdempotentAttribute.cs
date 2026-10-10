using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;

namespace PharmaSaaS.Presentation.Attributes;

public class IdempotentAttribute : ActionFilterAttribute
{
    public const string _headerName = "X-Idempotency-Key";
    private const string _inProgressValue = "in-progress";

    private static readonly HybridCacheEntryOptions _lockOptions = new()
    {
        Expiration = TimeSpan.FromSeconds(30),
        Flags = HybridCacheEntryFlags.DisableLocalCache
    };

    private static readonly HybridCacheEntryOptions _resultOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };

    private sealed record CachedResponse(int StatusCode, object? Value);

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out var idempotencyKey)
            || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            context.Result = new BadRequestObjectResult($"Missing or invalid {_headerName} header.");
            return;
        }

        var cache = context.HttpContext.RequestServices.GetRequiredService<HybridCache>();
        var cacheKey = $"idempotency:{idempotencyKey}";
        var cached = await cache.GetOrCreateAsync(cacheKey, _ => ValueTask.FromResult<string?>(null));

        switch (cached)
        {
            case _inProgressValue:
                context.Result = new StatusCodeResult(StatusCodes.Status409Conflict);
                return;

            case not null:
                var cachedResponse = JsonSerializer.Deserialize<CachedResponse>(cached);
                if (cachedResponse is not null)
                {
                    context.Result = new ObjectResult(cachedResponse.Value) { StatusCode = cachedResponse.StatusCode };
                    return;
                }
                break;
        }

        await cache.SetAsync(cacheKey, _inProgressValue, _lockOptions);

        var executedContext = await next();

        if (executedContext.Result is not ObjectResult objectResult)
        {
            await cache.RemoveAsync(cacheKey);
            return;
        }

        var statusCode = objectResult.StatusCode
            ?? (objectResult.Value is ProblemDetails pd ? pd.Status : null);

        if (statusCode is null)
        {
            await cache.RemoveAsync(cacheKey);
            return;
        }

        if (statusCode is >= 200 and < 300)
        {
            var response = new CachedResponse(statusCode.Value, objectResult.Value);
            var responseJson = JsonSerializer.Serialize(response);
            await cache.SetAsync(cacheKey, responseJson, _resultOptions);
        }
        else
        {
            await cache.RemoveAsync(cacheKey);
        }
    }
}