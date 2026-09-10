namespace PharmaSaaS.Application.Features.PurchaseOrders.GetItems;

public record GetPurchaseOrderItemsByStateQuery(int PurchaseOrderId, PurchaseOrderItemStatus State) : IRequest<Result<List<PurchaseOrderItemResponse>>>;
