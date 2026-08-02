namespace PharmaSaaS.Application.Contracts.Suppliers;

public record SupplierResponse(
    int Id,
    string Name,
    string? ContactInfo,
    decimal Balance,
    DateOnly? LastOrderDate
);
