namespace PharmaSaaS.Application.Contracts.Customers;

public record CustomerResponse(
    int Id,
    string FullName,
    string PhoneNumber,
    decimal TotalSpend,
    LoyaltyTierResponse? CurrentTier
);
