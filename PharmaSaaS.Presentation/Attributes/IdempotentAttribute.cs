using Microsoft.Extensions.Caching.Hybrid;
using PharmaSaaS.Application.Tenants;
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
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = $"Missing or invalid {_headerName} header.",
                Extensions = new Dictionary<string, object?>
                {
                    {
                        "error", new Dictionary<string, string>
                        {
                            { "code", "Idempotency.MissingHeader" },
                            { "description", $"Missing or invalid {_headerName} header." }
                        }
                    }
                }
            };

            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        var cache = context.HttpContext.RequestServices.GetRequiredService<HybridCache>();
        var tenantGetter = context.HttpContext.RequestServices.GetService<ICurrentTenantGetter>();
        var tenantId = tenantGetter?.GetTenantId() ?? "shared";
        var path = context.HttpContext.Request.Path.Value?.ToLowerInvariant();
        var method = context.HttpContext.Request.Method;
        var cacheKey = $"idempotency:{tenantId}:{method}:{path}:{idempotencyKey}";
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
                    context.Result = cachedResponse.Value is not null
                        ? new ObjectResult(cachedResponse.Value) { StatusCode = cachedResponse.StatusCode }
                        : new StatusCodeResult(cachedResponse.StatusCode);
                    return;
                }
                break;
        }

        await cache.SetAsync(cacheKey, _inProgressValue, _lockOptions);

        var executedContext = await next();

        int? statusCode = null;
        object? responseValue = null;

        if (executedContext.Result is ObjectResult objectResult)
        {
            statusCode = objectResult.StatusCode
                ?? (objectResult.Value is ProblemDetails pd ? pd.Status : null);
            responseValue = objectResult.Value;
        }
        else if (executedContext.Result is StatusCodeResult statusCodeResult)
        {
            statusCode = statusCodeResult.StatusCode;
            responseValue = null;
        }

        if (statusCode is >= 200 and < 300)
        {
            var response = new CachedResponse(statusCode.Value, responseValue);
            var responseJson = JsonSerializer.Serialize(response);
            await cache.SetAsync(cacheKey, responseJson, _resultOptions);
        }
        else
        {
            await cache.RemoveAsync(cacheKey);
        }
    }
}
