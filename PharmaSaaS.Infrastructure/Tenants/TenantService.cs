using Microsoft.AspNetCore.Http;

namespace PharmaSaaS.Infrastructure.Tenants;

public class TenantService(IHttpContextAccessor accessor, IOptions<TenantSettings> options) : ITenantService, ICurrentTenantGetter
{
    private readonly IHttpContextAccessor _accessor = accessor;
    private readonly TenantSettings _settings = options.Value;
    private Tenant? _currentTenant;

    public string? GetConnectionString()
        => GetCurrentTenant()?.ConnectionString;

    public Tenant? GetCurrentTenant()
    {
        if (_currentTenant is not null)
            return _currentTenant;

        var tenantId = _accessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(tenantId))
            return null;

        _currentTenant = _settings.Tenants.FirstOrDefault(t => t.Id == tenantId);
        return _currentTenant;
    }

    public string? GetTenantId()
        => GetCurrentTenant()?.Id;
}
