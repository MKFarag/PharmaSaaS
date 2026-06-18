namespace PharmaSaaS.Application.Tenants;

public interface ICurrentTenantGetter
{
    string? GetTenantId();
}
