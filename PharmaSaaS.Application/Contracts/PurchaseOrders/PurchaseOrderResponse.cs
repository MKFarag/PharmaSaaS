using PharmaSaaS.Application.Contracts.Suppliers;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderResponse(
    int Id,
    DateOnly OrderDate,
    decimal TotalAmount,
    string Status,
    SupplierResponse Supplier
);
