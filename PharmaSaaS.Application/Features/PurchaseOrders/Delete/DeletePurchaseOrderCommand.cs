namespace PharmaSaaS.Application.Features.PurchaseOrders.Delete;

public record DeletePurchaseOrderCommand(int Id) : IRequest<Result>;
