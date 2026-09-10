namespace PharmaSaaS.Application.Contracts.Inventories;

public record InventoryDetailResponse(
    int Id,
    float Quantity,
    decimal SellingPrice,
    int? TargetStock,
    bool IsRare,
    float TotalUnitsSold,
    DateOnly? LastSoldAt,
    DateOnly? LastOrderedAt,
    ProductResponse Product
);
