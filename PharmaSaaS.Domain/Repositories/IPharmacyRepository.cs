namespace PharmaSaaS.Domain.Repositories;

public interface IPharmacyRepository
{
    Task<IEnumerable<TProjection>> GetAllAsync<TProjection>(CancellationToken cancellationToken = default);
    Task<Pharmacy?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<Pharmacy?> FindByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string tenantId, string name, string excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pharmacy>> GetActiveWithExpiredSubscriptionAsync(DateOnly today, CancellationToken cancellationToken = default);
    Task AddAsync(Pharmacy pharmacy, CancellationToken cancellationToken = default);
    Task<SubscriptionPayment?> GetPaymentAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionPayment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
    Task AddPaymentAsync(SubscriptionPayment payment, CancellationToken cancellationToken = default);
}
