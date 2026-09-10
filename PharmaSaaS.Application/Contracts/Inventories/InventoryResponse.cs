namespace PharmaSaaS.Application.Contracts.Inventories;

public record InventoryResponse(
    int Id,
    string Name,
    string? Concentration,
    decimal SellingPrice,
    float Quantity,
    bool IsRare
);
