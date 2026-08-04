using PharmaSaaS.Application.Contracts.PurchaseOrders;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Get;

public record GetPurchaseOrderQuery(int Id) : IRequest<Result<PurchaseOrderDetailResponse>>;
