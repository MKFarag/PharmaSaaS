namespace PharmaSaaS.Infrastructure.Factories;

public class TenantProvisioningService(IOptions<TenantSettings> options)
{
    public async Task ProvisionAllTenantsAsync()
    {
        foreach (var tenant in options.Value.Tenants)
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(tenant.ConnectionString)
                .Options;

            await using var db = new ApplicationDbContext(
                dbOptions, new DesignTimeTenantService());

            await db.Database.MigrateAsync();
        }
    }
}
