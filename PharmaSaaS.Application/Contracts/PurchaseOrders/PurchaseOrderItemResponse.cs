namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    float Quantity,
    decimal UnitCost,
    float ReceivedQuantity,
    float ReturnedQuantity,
    string? ReturnReason,
    string Status
);
