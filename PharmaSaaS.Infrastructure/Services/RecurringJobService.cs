using PharmaSaaS.Infrastructure.Factories;

namespace PharmaSaaS.Infrastructure.Services;

public class RecurringJobService(IOptions<TenantSettings> tenantSettings, IUnitOfWork unitOfWork, INotificationService notificationService) : IRecurringJobService
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

    public async Task DeactivateExpiredPharmacySubscriptionsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expiredPharmacies = await unitOfWork.Pharmacies.GetActiveWithExpiredSubscriptionAsync(today);

        if (expiredPharmacies.Count == 0)
            return;

        foreach (var pharmacy in expiredPharmacies)
            pharmacy.Deactivate();

        await unitOfWork.SharedCompleteAsync();

        await notificationService.SendExpiredSubscriptionsReportAsync(
            expiredPharmacies.Select(p => p.Name).ToList().AsReadOnly());

        foreach (var pharmacy in expiredPharmacies)
            await notificationService.SendPharmacySubscriptionExpiredAsync(pharmacy.Email, pharmacy.OwnerName, pharmacy.Name, pharmacy.SubscriptionEnd!.Value);
    }
}
