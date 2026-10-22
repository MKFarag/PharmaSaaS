namespace PharmaSaaS.Application.Contracts.Pharmacies;

public record PharmacyResponse(
    string Id,
    string TenantId,
    string Name,
    DateOnly? SubscriptionStart,
    DateOnly? SubscriptionEnd,
    bool IsActive
);
