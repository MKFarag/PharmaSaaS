namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    float Quantity,
    decimal UnitCost
);
