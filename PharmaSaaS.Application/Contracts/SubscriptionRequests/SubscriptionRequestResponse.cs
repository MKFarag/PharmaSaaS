namespace PharmaSaaS.Application.Contracts.SubscriptionRequests;

public record SubscriptionRequestResponse(
    string Id,
    string PharmacyName,
    string OwnerName,
    string PhoneNumber,
    string Email,
    DateTime CreatedAt,
    SubscriptionRequestStatus Status
);
