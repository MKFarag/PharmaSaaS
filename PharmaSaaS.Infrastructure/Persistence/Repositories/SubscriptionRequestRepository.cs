namespace PharmaSaaS.Infrastructure.Persistence.Repositories;

internal sealed class SubscriptionRequestRepository(SharedDbContext context) : ISubscriptionRequestRepository
{
    public async Task<IEnumerable<TProjection>> GetAllAsync<TProjection>(SubscriptionRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = context.SubscriptionRequests.AsNoTracking();

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        return await query.ProjectToType<TProjection>().ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionRequest?> GetAsync(string id, CancellationToken cancellationToken = default)
        => await context.SubscriptionRequests.FindAsync([id], cancellationToken);

    public async Task AddAsync(SubscriptionRequest request, CancellationToken cancellationToken = default)
        => await context.SubscriptionRequests.AddAsync(request, cancellationToken);

    public async Task<bool> ExistsPendingAsync(string pharmacyName, string phoneNumber, string email, CancellationToken cancellationToken = default)
        => await context.SubscriptionRequests.AnyAsync(
            s => s.PharmacyName == pharmacyName &&
                 s.PhoneNumber == phoneNumber &&
                 s.Email == email &&
                 s.Status == SubscriptionRequestStatus.Pending, cancellationToken);
}
