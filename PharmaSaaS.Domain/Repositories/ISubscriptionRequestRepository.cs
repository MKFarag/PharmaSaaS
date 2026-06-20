namespace PharmaSaaS.Domain.Repositories;

public interface ISubscriptionRequestRepository
{
    Task<IEnumerable<TProjection>> GetAllAsync<TProjection>(string? status = null, CancellationToken cancellationToken = default);
    Task<SubscriptionRequest?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(SubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExistsPendingAsync(string pharmacyName, string phoneNumber, string email, CancellationToken cancellationToken = default);
}
