namespace PharmaSaaS.Application.Features.PurchaseOrders.Update;

public record UpdatePurchaseOrderCommand(int Id, UpdatePurchaseOrderRequest Request) : IRequest<Result<PurchaseOrderResponse>>;
