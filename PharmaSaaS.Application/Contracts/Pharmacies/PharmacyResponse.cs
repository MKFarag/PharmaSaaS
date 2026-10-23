namespace PharmaSaaS.Application.Contracts.Pharmacies;

public record PharmacyResponse(
    string Id,
    string TenantId,
    string Name,
    string OwnerName,
    string PhoneNumber,
    string Email,
    DateOnly? SubscriptionStart,
    DateOnly? SubscriptionEnd,
    bool IsActive
);
