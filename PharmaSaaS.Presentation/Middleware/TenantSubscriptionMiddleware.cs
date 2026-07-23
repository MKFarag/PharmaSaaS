using PharmaSaaS.Infrastructure.Tenants;

namespace PharmaSaaS.Presentation.Middleware;

public class TenantSubscriptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    private static readonly string[] _excludedPaths =
        ["/swagger", "/redoc", "/health", "/jobs"];

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        if (_excludedPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var isSystemOwner = endpoint?.Metadata.GetMetadata<SystemOwnerAttribute>() is not null;

        if (isSystemOwner)
        {
            await _next(context);
            return;
        }

        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "X-Tenant-Id header is required" });
            return;
        }

        var tenant = tenantService.GetCurrentTenant();

        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not found" });
            return;
        }

        if (tenant.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Subscription expired",
                expiredAt = tenant.EndDate.ToString("yyyy-MM-dd")
            });
            return;
        }

        await _next(context);
    }
}