namespace PharmaSaaS.Application.Interfaces.Services;

public interface IPurchaseOrderService
{
    Task<Result> ApproveItemsAsync(PurchaseOrder purchaseOrder, IEnumerable<ApprovePurchaseOrderItemRequest> requestedItems, IUnitOfWork unitOfWork, CancellationToken cancellationToken = default);

    Task<Result> ReturnItemsAsync(PurchaseOrder purchaseOrder, IEnumerable<ReturnPurchaseOrderItemRequest> requestedItems, IUnitOfWork unitOfWork, CancellationToken cancellationToken = default);

    void AddItems(PurchaseOrder purchaseOrder, IEnumerable<PurchaseOrderItemRequest> addedItems);
}
