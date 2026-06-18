namespace PharmaSaaS.Infrastructure.Tenants;

public interface ITenantService
{
    string? GetConnectionString();
    Tenant? GetCurrentTenant();
}
