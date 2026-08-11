using PharmaSaaS.Application.Contracts.Suppliers;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderResponse(
    int Id,
    DateOnly OrderDate,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    decimal CalculatedAmount,
    decimal ReturnedAmount,
    string Status,
    SupplierResponse Supplier
);
