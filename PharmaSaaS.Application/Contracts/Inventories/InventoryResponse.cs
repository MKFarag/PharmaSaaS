namespace PharmaSaaS.Application.Contracts.Inventories;

public record InventoryResponse(
    int Id,
    float Quantity,
    decimal SellingPrice,
    int? TargetStock,
    bool IsRare,
    ProductResponse Product
);
