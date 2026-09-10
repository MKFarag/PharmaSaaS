namespace PharmaSaaS.Application.Features.PurchaseOrders.Return;

public record ReturnPurchaseOrderItemsCommand(int Id, ReturnPurchaseOrderItemsRequest Request) : IRequest<Result>;
