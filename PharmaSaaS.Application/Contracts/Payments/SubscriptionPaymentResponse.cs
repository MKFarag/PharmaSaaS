namespace PharmaSaaS.Application.Contracts.Payments;

public record SubscriptionPaymentResponse(
    int Id,
    string PharmacyId,
    string PharmacyName,
    string ImageUrl,
    bool IsVerified,
    decimal? Amount,
    DateTime SubmittedAt
);
