using Microsoft.AspNetCore.Mvc.Controllers;
using PharmaSaaS.Infrastructure.Tenants;

namespace PharmaSaaS.Presentation.Middleware;

public class TenantSubscriptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var endpoint = context.GetEndpoint();
        var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (actionDescriptor is null)
        {
            await _next(context);
            return;
        }

        var isSystemOwner = endpoint!.Metadata.GetMetadata<SystemOwnerAttribute>() is not null;
        var isNoTenantRequired = endpoint.Metadata.GetMetadata<NoTenantRequiredAttribute>() is not null;

        if (isSystemOwner || isNoTenantRequired)
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