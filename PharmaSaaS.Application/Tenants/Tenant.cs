namespace PharmaSaaS.Application.Tenants;

public class Tenant
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateOnly EndDate { get; set; }
    public string? ConnectionString { get; set; } = string.Empty;
}
