namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface IRecurringJobService
{
    Task RemoveExpiredRefreshTokenAsync();
    Task DeactivateExpiredPharmacySubscriptionsAsync();
}
