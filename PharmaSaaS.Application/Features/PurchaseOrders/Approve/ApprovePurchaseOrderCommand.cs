namespace PharmaSaaS.Application.Features.PurchaseOrders.Approve;

public record ApprovePurchaseOrderCommand(int Id, ApprovePurchaseOrderRequest Request) : IRequest<Result>;
