namespace PharmaSaaS.Application.Features.PurchaseOrders.Receive;

public record ReceivePurchaseOrderCommand(int Id) : IRequest<Result>;
