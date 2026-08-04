using PharmaSaaS.Application.Contracts.Suppliers;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderDetailResponse(
    int Id,
    DateOnly OrderDate,
    decimal TotalAmount,
    string Status,
    SupplierResponse Supplier,
    IList<PurchaseOrderItemResponse> Items
);
