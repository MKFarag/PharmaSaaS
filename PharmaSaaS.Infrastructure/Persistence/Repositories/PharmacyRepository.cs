namespace PharmaSaaS.Infrastructure.Persistence.Repositories;

public class PharmacyRepository(SharedDbContext context) : IPharmacyRepository
{
    public async Task<IEnumerable<TProjection>> GetAllAsync<TProjection>(CancellationToken cancellationToken = default)
        => await context.Pharmacies.AsNoTracking().ProjectToType<TProjection>().ToListAsync(cancellationToken);

    public async Task<Pharmacy?> GetAsync(string id, CancellationToken cancellationToken = default)
        => await context.Pharmacies.FindAsync([id], cancellationToken);

    public async Task<Pharmacy?> FindByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
        => await context.Pharmacies.FirstOrDefaultAsync(p => p.TenantId == tenantId, cancellationToken);

    public async Task<bool> NameExistsAsync(string tenantId, string name, string excludeId, CancellationToken cancellationToken = default)
        => await context.Pharmacies.AnyAsync(p => p.TenantId == tenantId && p.Name == name && p.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Pharmacy>> GetActiveWithExpiredSubscriptionAsync(DateOnly today, CancellationToken cancellationToken = default)
        => await context.Pharmacies
            .Where(p => p.IsActive && p.SubscriptionEnd.HasValue && p.SubscriptionEnd.Value < today)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Pharmacy pharmacy, CancellationToken cancellationToken = default)
        => await context.Pharmacies.AddAsync(pharmacy, cancellationToken);
}
