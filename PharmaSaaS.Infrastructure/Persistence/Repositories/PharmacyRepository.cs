namespace PharmaSaaS.Infrastructure.Persistence.Repositories;

public class PharmacyRepository(SharedDbContext context) : IPharmacyRepository
{
    public async Task<IEnumerable<TProjection>> GetAllAsync<TProjection>(CancellationToken cancellationToken = default)
        => await context.Pharmacies.AsNoTracking().ProjectToType<TProjection>().ToListAsync(cancellationToken);

    public async Task<Pharmacy?> GetAsync(int id, CancellationToken cancellationToken = default)
        => await context.Pharmacies.FindAsync([id], cancellationToken);

    public async Task<bool> NameExistsAsync(string tenantId, string name, int excludeId, CancellationToken cancellationToken = default)
        => await context.Pharmacies.AnyAsync(p => p.TenantId == tenantId && p.Name == name && p.Id != excludeId, cancellationToken);
}
