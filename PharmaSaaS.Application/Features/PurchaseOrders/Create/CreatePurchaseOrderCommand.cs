namespace PharmaSaaS.Application.Features.PurchaseOrders.Create;

public record CreatePurchaseOrderCommand(PurchaseOrderRequest Request) : IRequest<Result<PurchaseOrderResponse>>;
