using PharmaSaaS.Infrastructure.Factories;

namespace PharmaSaaS.Infrastructure.Services;

public class RecurringJobService(IOptions<TenantSettings> tenantSettings) : IRecurringJobService
{
    public async Task RemoveExpiredRefreshTokenAsync()
    {
        foreach (var tenant in tenantSettings.Value.Tenants)
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(tenant.ConnectionString)
                .Options;

            await using var db = new ApplicationDbContext(dbOptions, new DesignTimeTenantService());

            await db.Users
                .SelectMany(u => u.RefreshTokens)
                .Where(t => t.ExpiresOn < DateTime.UtcNow)
                .ExecuteDeleteAsync();
        }
    }
}