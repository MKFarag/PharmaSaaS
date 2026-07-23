namespace PharmaSaaS.Application.Contracts.Customers;

public record CustomerResponse(
    int Id,
    string FullName,
    string PhoneNumber,
    decimal TotalSpend,
    int? CurrentTierId,
    string? CurrentTierName
);
