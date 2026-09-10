namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderResponse(
    int Id,
    int SupplierId,
    string SupplierName,
    decimal CalculatedAmount,
    DateOnly OrderDate,
    string Status,
    decimal RemainingAmount
);
