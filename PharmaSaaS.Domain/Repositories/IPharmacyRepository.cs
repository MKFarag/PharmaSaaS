namespace PharmaSaaS.Domain.Repositories;

public interface IPharmacyRepository
{
    Task<IEnumerable<TProjection>> GetAllAsync<TProjection>(CancellationToken cancellationToken = default);
    Task<Pharmacy?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Pharmacy?> FindByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string tenantId, string name, int excludeId, CancellationToken cancellationToken = default);
    Task AddAsync(Pharmacy pharmacy, CancellationToken cancellationToken = default);
}
