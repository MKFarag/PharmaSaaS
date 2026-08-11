using PharmaSaaS.Application.Contracts.Suppliers;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderDetailResponse(
    int Id,
    DateOnly OrderDate,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    string Status,
    SupplierResponse Supplier,
    IList<PurchaseOrderItemResponse> Items
);
